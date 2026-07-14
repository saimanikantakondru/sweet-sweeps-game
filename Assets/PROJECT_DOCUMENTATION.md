# Sweet Sweeps — "Coin Calamity" Project Documentation

A Unity **real-money wagering game** built for WebGL/browser. The player places a bet,
then plays a 2D platformer round where coins spawn, calamities happen, and the player
tries to survive and collect. **The server is authoritative**: it decides everything
about the round outcome (how many coins spawn per step, when calamities start, the
final payout). The client only renders the round and reports what was collected —
the server validates every claim. This is the standard architecture for gambling
games so players cannot cheat.

---

## 1. Tech Stack

| Piece | Purpose |
|---|---|
| **Unity (2D, URP)** | Engine — Spine animations, Cinemachine camera, Input System |
| **VContainer** | Dependency injection — wires all services together via interfaces |
| **UniTask** | async/await support for Unity |
| **Colyseus SDK** (`Assets/Colyseus/`) | WebSocket client for real-time round streaming |
| **UnityWebRequest** | Plain HTTPS REST calls (money transactions) |

**Server endpoints** (configured in `Assets/Resources/ServerSettings` ScriptableObject,
class: `Assets/Scripts/Server/Data/ServerSettings.cs`):

- REST API: `https://stage.qubitgamez.com/api/mrgs/games`
- WebSocket: `wss://stage.qubitgamez.com/api/mrgs/games`
- Request timeout: 5 seconds

**Scenes:** `Bootstrap.unity` → `Menu.unity` → `Gameplay.unity`

---

## 2. Full Flow Diagram

```
Bootstrap scene          Menu scene                        Gameplay scene
──────────────           ─────────────────────────         ──────────────────────────────
Password gate    ──►     ResolveToken (URL ?token= )
                         POST /initialize  ──────────►     (wallets, bet ladder, unfinished games)
                         [PLAY pressed]
                         POST /play action=start  ──►      (gamePlayUid, totalSteps, wager deducted)
                                                  ──►      Load Gameplay scene
                                                           WS JoinOrCreate("coincalamity", {gamePlayUid})
                                                           ◄── round_config (totalSteps, coin values)
                                                           ◄── step × N  (1/sec: what to spawn)
                                                           ◄── calamity_values
                                                           [player plays, collects, maybe dies]
                                                           ──► player_death (if died), WS Leave
                                                           POST /play action=complete + report
                                                           ◄── totalWin + updated wallet
                                                           back to Menu / next round
```

HTTP handles the **transactional** money operations; the Colyseus WebSocket handles
the **real-time** round streaming. The two worlds are linked by one ID: `gamePlayUid`.

---

## 3. Step 1 — App Start: Bootstrap Scene

### PersistentLifetimeScope (`Assets/Scripts/Installers/PersistentLifetimeScope.cs`)
The root DI container. Marked `DontDestroyOnLoad`, so it survives all scene changes.
Registers all network singletons:

- `GameApiService` — REST client, built with the URL from `Resources/ServerSettings`
- `ColyseusWebSocketService` — WebSocket client
- `GameSessionService` — holds token, wallets, current round data
- `SessionInitializer`, `SceneService`, audio services, message box, loading screen

### BootstrapController (`Assets/Scripts/Bootstrap/BootstrapController.cs`)
Shows a **password gate** (dev/stage protection screen). Input is SHA-256 hashed
(`Security/Sha256PasswordHasher.cs`) and compared against a hardcoded hash.
On success → `SceneService.LoadMenuAsync()` loads the Menu scene.

---

## 4. Step 2 — Menu Scene: Authentication + `/initialize`

Entry point: `Assets/Scripts/Menu/UI/MenuPresenter.cs` (`Start()`).

### 4a. Token resolution — `SessionInitializer.ResolveToken()`
- On WebGL, reads `?token=...` from the **browser URL** via JavaScript interop
  (`Assets/Scripts/Infrastructure/UrlParams.cs`, `[DllImport("__Internal")]`).
  This is how a casino/operator site passes the logged-in player identity to the game.
- If no URL token (e.g. in Editor), generates and persists a **demo token**
  like `DEMO-12345` in PlayerPrefs.
- `brand` and `game` can also come from URL params (defaults:
  `sweetsweeps-dev` / `coin-calamity`).

### 4b. First API call — `POST /initialize`
Request body:
```json
{ "token": "...", "brand": "sweetsweeps-dev", "game": "coin-calamity" }
```
(Optionally includes `currency` when the player switches currency.)

Response (`InitializeResponse` in `Assets/Scripts/Server/ServerAPI_Types.cs`):

| Field | Meaning |
|---|---|
| `session` | Session UID — used as the token for later `/play` calls |
| `wallet` / `wallets` | Player balances per currency |
| `settings.stakes` | The **bet ladder** (allowed wager amounts) shown in the menu |
| `unfinishedGames` | Rounds the player paid for but never finished → offered as resume |

Stored in `GameSessionService`, which fires `OnSessionInitialized` /
`OnWalletsChanged` events so the UI (balance, bet selector) refreshes.
On failure, the menu shows a "Retry" message box in a loop until success.

### How the REST client works — `GameApiService.Post<T>()`
(`Assets/Scripts/Server/Services/GameApiService.cs`)
1. Serializes the request object with `JsonUtility.ToJson`.
2. Sends a `UnityWebRequest` POST with `Content-Type: application/json`.
3. Awaits with UniTask + a 5-second timeout `CancellationTokenSource`.
4. Converts every failure into a typed `ApiException` with helpers:
   `IsNetworkError` (code 0), `IsAuthError` (401), `IsClientError` (4xx),
   `IsServerError` (5xx) — so callers can show the right error dialog.

---

## 5. Step 3 — PLAY Pressed: `POST /play` (action=start)

`MenuPresenter.HandlePlayPressed()`:

1. Checks balance/auth, then calls
   `apiService.StartRound(sessionUid, "coin-calamity", selectedBet)`
   → `POST /play` with `action: "start"` and the wager.
   **This is the moment money is deducted from the wallet.**
2. Response (`PlayStartResponse`) contains the round blueprint, decided server-side:
   - `gamePlay` — **GamePlayUid**, the key used to join the WebSocket room later
   - `gameRound` — uid used later in the complete call
   - `response.totalSteps`, `response.calamityStartStep`, `response.levelPreset`
   - updated `wallet`
3. `GameSessionService.ApplyStartResponse()` stores it, then
   `MenuService.Play()` → `SceneService.LoadGameplayAsync()`.

Special cases:
- HTTP **402** → "Insufficient Balance" popup.
- If `HasUnfinishedGame`, the start call is **skipped** — the game resumes the
  existing paid round (bet selector renders locked at the original wager).

---

## 6. Step 4 — Gameplay Scene: WebSocket Connection

Scene entry point: `Assets/Scripts/Core/GameBootstrap.cs` (with a child DI scope,
`GameplayLifetimeScope`). On boot it:

1. Loads/builds the level (`LevelCoordinator`), places the player, disables input.
2. Calls `RoundCoordinator.PrepareRound()` which either runs an **offline
   simulation** (Editor testing, `GameSettingsSO.UseSimulation`) or calls
   **`NetworkCoordinator.Connect()`**.
3. Starts the game state machine countdown
   (`Idle → Countdown → Playing → GameOver → Resetting`,
   in `Assets/Scripts/Core/StateMachine/`).

### The connection — `ColyseusWebSocketService.Connect()`
(`Assets/Scripts/Server/Services/ColyseusWebSocketService.cs`)

```csharp
_client = new ColyseusClient("wss://stage.qubitgamez.com/api/mrgs/games");
_room   = await _client.JoinOrCreate<object>("coincalamity",
              new Dictionary<string, object> { { "gamePlayUid", gamePlayUid } });
```

- Joins (or creates) a Colyseus **room** named `"coincalamity"`, passing the
  `gamePlayUid` from the start call as the join option. That is how the server
  knows which *paid round* this socket belongs to.
- Under the hood the Colyseus SDK does an HTTP matchmaking request first, gets a
  room/session ID, then opens the actual WebSocket and speaks Colyseus's binary
  (MsgPack) protocol.

### Messages received (client ◄── server)

| Message | Payload class | Meaning |
|---|---|---|
| `round_config` | `SessionDataJson` | Round setup: `totalSteps`, purple-coin values, `reconnected` flag |
| `step` | `StepDataJson` | One tick: counts of `gold_coins`, `purple_coins`, `sour_candies`, `spiked_candies`, `meteors` to spawn + current `phase` |
| `calamity_values` | `CalamityValuesJson` | Purple-coin values during calamity |
| `round_end` | — | Server signals round timing done (currently informational; round ends by timer) |

(Payload classes: `Assets/Scripts/Server/Data/StepDataJson.cs`,
`Assets/Scripts/Server/Data/CalamityValuesJson.cs`)

### Messages sent (client ──► server)

| Message | Payload | When |
|---|---|---|
| `player_death` | `CollectionReport` | When the player dies mid-round |

### Lifecycle handling
- `Room.OnLeave` — leave code `4000` = expected/consented leave → `OnDisconnected`.
  Any other code → `OnConnectionFailed` ("Connection lost").
- `Room.OnError` → `OnError` + `OnConnectionFailed`.
- `SimulateUnexpectedDrop()` / debug panel — QA tools to test disconnect handling.

---

## 7. Step 5 — How Server Data Drives Gameplay

### NetworkCoordinator (`Assets/Scripts/Core/NetworkCoordinator.cs`)
The translator between raw socket events and game systems:
- `round_config` → `StepService.StartSession()` (total steps, round duration)
  and `PurpleCoinValueService.SetNormalValues()`.
- each `step` → `StepService.ReceiveStep()`.
- `calamity_values` → `PurpleCoinValueService.SetCalamityValues()`.
- Subscribes/unsubscribes these handlers **per round** so stale events can't leak
  between rounds.

### StepService (`Assets/Scripts/Core/Services/StepService.cs`)
The heart of the loop — a VContainer `ITickable` (runs every frame):

1. **Buffering.** Incoming steps go into a queue. Playback only begins once a
   pre-buffer of steps has arrived (`GameSettingsSO.StepPrebufferCount`) — this
   smooths network jitter.
2. **Playback.** Every `stepInterval` (1 second) it dequeues one step and:
   - tells `PhaseService` about phase changes (normal → calamity),
   - tells `CollectibleService` to **spawn exactly what the server dictated**
     for that step,
   - fires `OnStepReceived` for HUD, audio, etc.
3. **Stall detection.** If steps stop arriving for
   `GameSettingsSO.StepStreamStallTimeout` seconds while more are expected →
   `OnStreamStalled` → `NetworkCoordinator` treats it as connection loss →
   `GameBootstrap.OnNetworkConnectionFailed()` pauses the game, shows
   "Connection Lost", and returns to menu.
4. When `CurrentStep >= TotalSteps` → `OnAllStepsComplete` → round over.

Meanwhile the player runs/jumps (`Gameplay/Player/PlayerController.cs`), collects
spawned coins (`Server/Services/CollectionTracker.cs` counts them locally), and
takes damage from hazards (`HealthService`).

---

## 8. Step 6 — Round End: `POST /play` (action=complete)

The round ends when all steps are processed or the player dies. The state machine
enters `GameOverState`, and `GameBootstrap.OnGameOver()` calls
`RoundCoordinator.CompleteRoundAsync()` (`Assets/Scripts/Core/RoundCoordinator.cs`):

1. Builds a **`CollectionReport`** — gold/purple coins collected, survived yes/no,
   survival time (`currentStep * 1000ms + 2000ms offset`).
2. If the player died → sends `player_death` over the **WebSocket** first,
   then disconnects the socket cleanly (`room.Leave()`).
3. Calls `POST /play` with `action: "complete"`, the `gameRound` uid, and the
   report. The **server validates the claim** (it knows exactly what it spawned,
   so it can verify you didn't claim more coins than existed — see
   `validationStatus` on `GameActivityData`) and returns `totalWin` + updated wallet.
4. `ApplyCompleteResponse()` updates the wallet → Game Over screen shows winnings
   → back to menu, or `ResettingState` → `PrepareRound()` for another round.

**Failure handling:** if the complete call fails (network/server error), the result
is flagged failed and the player returns to the menu — where the next `/initialize`
reports the round as an **unfinished game** and offers to resume it, locked to the
original wager.

---

## 9. REST API Reference (client-side view)

Base URL: `https://stage.qubitgamez.com/api/mrgs/games`
Request/response classes: `Assets/Scripts/Server/ServerAPI.cs` and
`Assets/Scripts/Server/ServerAPI_Types.cs`.

| Endpoint | Request | Response | Purpose |
|---|---|---|---|
| `POST /initialize` | `token`, `brand`, `game`, (`currency`) | session, wallets, stakes ladder, unfinished games | Authenticate + load session |
| `POST /play` (start) | `token`(=session), `game`, `action:"start"`, `wager` | `gamePlay`, `gameRound`, `totalSteps`, `calamityStartStep`, `levelPreset`, wallet | Place bet, get round blueprint |
| `POST /play` (complete) | `token`, `game`, `gameRound`, `action:"complete"`, `data`(CollectionReport) | `totalWin`, activities with `validationStatus`, wallet | Settle round, get payout |

Error body shape: `{ "status": "...", "code": 123, "message": "..." }` →
parsed into `ApiException`.

---

## 10. Key Architecture Ideas

- **Dependency injection everywhere.** Services depend only on interfaces
  (`IGameApiService`, `IWebSocketService`, `IGameSessionService`, …) registered in
  the `Installers/` lifetime scopes. Nothing news-up network classes directly.
- **Three-scope DI hierarchy.** `PersistentLifetimeScope` (network/session,
  survives scenes) → `MenuLifetimeScope` / `GameplayLifetimeScope` (per-scene
  services and coordinators).
- **Server-authoritative rounds.** The client never decides spawns or payouts;
  it replays a server-generated script and reports collections for validation.
- **HTTP for money, WebSocket for real-time.** Linked by `gamePlayUid`.
- **Simulation mode.** `GameSettingsSO.UseSimulation` lets the whole game run
  offline in the Editor with `SimulationCoordinator` faking the step stream —
  useful for gameplay iteration without a server.
- **Resilience.** Request timeouts, retry dialogs, step-stream stall detection,
  unexpected-disconnect codes, and unfinished-game resume all protect the player's
  money if anything drops mid-round.

---

## 11. Modification Guide — Where to Change What

Two kinds of changes exist in this project:

- **Asset changes (no code):** most tuning lives in **ScriptableObject assets** under
  `Assets/Data/ScriptableObjects/` and `Assets/Resources/`. Select the asset in the
  Unity Project window and edit values in the Inspector.
- **Script changes:** behavior/logic lives in `Assets/Scripts/`. The table below maps
  each change to its script.

### 11.1 Game start (Bootstrap)

| What you want to change | Where |
|---|---|
| The startup password | `Assets/Scripts/Bootstrap/BootstrapController.cs` → `_expectedHash` field. Compute the SHA-256 hash of your new password (lowercase hex) and replace the string. |
| Password screen texts/buttons | `Bootstrap.unity` scene → BootstrapController inspector fields (`defaultFeedback`, `accessGranted`, `accessDenied`) |
| Skip the password gate entirely | In `BootstrapController.OnSubmit()` remove the validator check, or call `_sceneService.LoadMenuAsync()` directly from `Start()` |
| Which scene loads first / scene order | `File → Build Settings` scene list; scene-loading logic in `Assets/Scripts/Core/Services/SceneService.cs` |

### 11.2 Server connection (API + WebSocket)

| What you want to change | Where |
|---|---|
| **API base URL / WebSocket URL / request timeout** | **Asset:** `Assets/Resources/ServerSettings.asset` (Inspector). Script defaults: `Assets/Scripts/Server/Data/ServerSettings.cs` |
| Brand / game identifiers sent to server | Defaults `"sweetsweeps-dev"` / `"coin-calamity"` are in `Assets/Scripts/Server/ServerAPI.cs` (request classes) and `Assets/Scripts/Server/Services/SessionInitializer.cs` (constants). At runtime they can also be overridden by URL params `?brand=` and `?game=`. |
| Colyseus room name (`"coincalamity"`) | `Assets/Scripts/Server/Services/ColyseusWebSocketService.cs` → `JoinOrCreate<object>("coincalamity", options)` |
| **Add a new API endpoint** | 1) Add request/response classes in `ServerAPI.cs` / `ServerAPI_Types.cs`. 2) Add a method to `Assets/Scripts/Server/Contracts/IGameApiService.cs`. 3) Implement it in `GameApiService.cs` (reuse the private `Post<T>()`). 4) Call it from a presenter/coordinator. |
| **Add a new WebSocket message (receive)** | 1) Add a payload class in `Assets/Scripts/Server/Data/`. 2) Add an event to `Assets/Scripts/Server/Contracts/IWebSocketService.cs`. 3) Subscribe in `ColyseusWebSocketService.SubscribeRoomMessages()` (`_room.OnMessage<T>("name", ...)`). 4) Handle it in `Assets/Scripts/Core/NetworkCoordinator.cs` (subscribe in `SubscribePerRound()`). |
| **Add a new WebSocket message (send)** | Add a method to `IWebSocketService` + `ColyseusWebSocketService` using `_room.Send("message_name", payload)`; expose it via `NetworkCoordinator` (see `SendPlayerDeath` as the template). |
| Timeout / error-handling behavior of API calls | `Assets/Scripts/Server/Services/GameApiService.cs` (`Post<T>()`, `ApiException`) |
| Reconnect / disconnect handling | `ColyseusWebSocketService.SubscribeRoomLifecycle()` (leave codes; `4000` = expected) and `NetworkCoordinator` (`OnWebSocketDisconnected`, `OnConnectionFailed`) |

### 11.3 Menu (bets, wallet, currencies)

| What you want to change | Where |
|---|---|
| Bet ladder values | Comes from the **server** (`/initialize` → `settings.stakes`). Client fallback array: `Assets/Scripts/Menu/Services/MenuService.cs` → `ApplyLadder()` |
| Bet selection rules (min/max/affordability) | `MenuService.cs` (`CanIncrement`, `CanPlay`, `ClampIndexToAffordable`, …) |
| What happens when PLAY is pressed | `Assets/Scripts/Menu/UI/MenuPresenter.cs` → `HandlePlayPressed()` |
| Menu UI layout/widgets | `Menu.unity` scene + views in `Assets/Scripts/Menu/UI/` (`TopBarView`, `BottomBarView`, `BetSelectorView`, `BetLadderPopupView`, `CurrencySelectorView`, …) |
| Currency icons | **Asset:** `Assets/Data/ScriptableObjects/CurrencyVisuals.asset` |
| Menu music / UI sounds | **Assets:** `Assets/Data/ScriptableObjects/Audio/MenuAudioSettings.asset`, `SoundDefinitions/snd_ui_click.asset` |

### 11.4 Round flow & timing

| What you want to change | Where |
|---|---|
| **Simulation mode on/off** (play offline without server), test balance | **Asset:** `Assets/Data/ScriptableObjects/GameSettings.asset` → `useSimulation`, `simulationBalance`. **Must be OFF for real server builds.** |
| Step interval, round duration, end offset | Same asset → `stepDurationSeconds`, `roundDuration`, `roundEndOffset` (script: `Assets/Scripts/Data/GameSettingsSO.cs`) |
| Network buffering / stall timeout | Same asset → `stepPrebufferCount`, `stepStreamStallTimeout` |
| Countdown length/speed | Same asset → `countdownFrom`, `countdownStepSeconds` |
| Fake server data for simulation | **Asset:** `Assets/Data/ScriptableObjects/StepSimulationPreset.asset`; logic in `Assets/Scripts/Core/SimulationCoordinator.cs` |
| Round prepare/stop/complete logic | `Assets/Scripts/Core/RoundCoordinator.cs` |
| Step playback / spawning-per-step logic | `Assets/Scripts/Core/Services/StepService.cs` (`Tick()`, `ProcessStep()`) |
| Game state flow (countdown → playing → game over → reset) | `Assets/Scripts/Core/StateMachine/GameStateMachine.cs` + states in `Core/StateMachine/States/`. To add a state: create the class, register it in `Assets/Scripts/Installers/GameplayInstaller.cs` / `StateMachineInitializer.cs`, wire events in `GameBootstrap.cs` |

### 11.5 Player

| What you want to change | Where |
|---|---|
| Move speed, jump force, health, physics feel | **Asset:** `Assets/Data/ScriptableObjects/PlayerStats.asset` (script: `Assets/Scripts/Data/PlayerStatsSO.cs`) |
| Movement/jump behavior code | `Assets/Scripts/Gameplay/Player/PlayerMover.cs`, `PlayerJumper.cs`, `PlayerController.cs` |
| Keyboard/gamepad bindings | `PlayerInputActions` input-actions asset (Unity Input System); providers in `Assets/Scripts/Gameplay/Input/` |
| Damage, invincibility frames, death | `Assets/Scripts/Core/Services/HealthService.cs`, `InvincibilityService.cs`; pit fall damage in `GameSettings.asset` → `pitDamage` + `Gameplay/Player/PitDetector.cs` |
| Respawn behavior | `Assets/Scripts/Gameplay/Player/RespawnService.cs` |
| Player animations | `Assets/Scripts/Gameplay/Player/PlayerAnimatorController.cs` + Spine assets in `Assets/Spine/` |

### 11.6 Collectibles, calamities, levels

| What you want to change | Where |
|---|---|
| Coin/candy values, prefabs, visuals | **Assets:** `GoldCoinsData.asset`, `PurpleCoinsData.asset`, `SourCandiesData.asset`, `SpikedCandiesData.asset` (script: `Assets/Scripts/Data/CollectibleDataSO.cs`) |
| How/where collectibles spawn | **Asset:** `SpawnSystemSettings.asset`; scripts in `Assets/Scripts/Gameplay/Collectibles/` (`SpawnPoolService`, `SpawnPointSelector`, `CollectibleService`) |
| Calamity strength/thresholds/visuals | **Assets:** `Assets/Data/ScriptableObjects/Calamity/*.asset`; behavior scripts in `Assets/Scripts/Gameplay/Calamities/` (e.g. `FrostCalamity.cs`, `HeavyCalamityEffect.cs`) |
| Add a new calamity type | Implement `ICalamity`/`ICalamityEffect` (`Assets/Scripts/Core/Contracts/`), create a `...CalamityDataSO` asset, register in `CalamityService` / gameplay installer, bind visuals via `CalamityBinding.cs` |
| Level layouts / presets / worlds | **Assets:** `LevelPreset_Biome*.asset`, `LevelCatalog.asset`, `World1Data.asset`, `World2Data.asset`; scripts in `Assets/Scripts/Gameplay/Level/` (`LevelService`, `LevelContentRoot`); orchestration in `Assets/Scripts/Core/LevelCoordinator.cs`. Note: which preset a round uses is chosen by the **server** (`levelPreset` in the start response). |
| Which world is playable / default world | `GameSettings.asset` → `availableWorldIds`; fallback `startWorldId` on `GameBootstrap` in `Gameplay.unity` |

### 11.7 UI, audio, misc

| What you want to change | Where |
|---|---|
| HUD (score, timer, health) | `Assets/Scripts/Infrastructure/UI/HUDView.cs`, wired by `UIPresenter.cs` |
| Game-over screen (winnings display) | `Assets/Scripts/Infrastructure/UI/GameOverView.cs` |
| Countdown/loading/message-box visuals | `CountdownView.cs`, `LoadingScreenView.cs`, `MessageBoxView.cs` in `Infrastructure/UI/` |
| All gameplay sounds/music | **Assets:** `Assets/Data/ScriptableObjects/Audio/` (SfxLibrary, SoundDefinitions, mixer config); coordinators in `Assets/Scripts/Core/*AudioCoordinator.cs` |
| Score / win-value math shown in HUD | `Assets/Scripts/Core/Services/ScoreService.cs`, `PurpleCoinValueService.cs` (values themselves come from the server) |
| Local save encryption | `Assets/Scripts/Infrastructure/EncryptedStorage.cs`, `AesCbcEncryption.cs`; toggle in `GameSettings.asset` → `useEncryptedStorage` |
| Debug cheats panel | `Assets/Scripts/Cheats/SROptions.cs`, `CheatBinder.cs` (SRDebugger); network-drop test panel: `Infrastructure/UI/DebugNetworkPanelView.cs` |

### 11.8 Adding a brand-new service (the pattern everything follows)

1. Define an interface in `Assets/Scripts/Core/Contracts/` (or `Server/Contracts`, `Menu/Contracts`).
2. Implement it in the matching `Services/` folder.
3. Register it in the right lifetime scope in `Assets/Scripts/Installers/`:
   - `PersistentLifetimeScope` — survives all scenes (network, session, audio)
   - `MenuLifetimeScope` — menu-only
   - `GameplayLifetimeScope` / `GameplayInstaller` — gameplay-only
4. Receive it anywhere via constructor injection or a `[Inject] Construct(...)` method — never `new` it or use singletons.

> **Golden rule:** anything about money, spawn counts, or payouts cannot be
> meaningfully changed client-side — the server validates it. Client changes affect
> presentation, feel, and flow only.

---

## 12. Where to Look (file map)

| Area | Path |
|---|---|
| Password gate | `Assets/Scripts/Bootstrap/BootstrapController.cs` |
| DI registration | `Assets/Scripts/Installers/` |
| REST client | `Assets/Scripts/Server/Services/GameApiService.cs` |
| WebSocket client | `Assets/Scripts/Server/Services/ColyseusWebSocketService.cs` |
| Session/wallet state | `Assets/Scripts/Server/Services/GameSessionService.cs` |
| Token/init flow | `Assets/Scripts/Server/Services/SessionInitializer.cs` |
| API request types | `Assets/Scripts/Server/ServerAPI.cs`, `ServerAPI_Types.cs` |
| Socket ↔ game glue | `Assets/Scripts/Core/NetworkCoordinator.cs` |
| Round lifecycle | `Assets/Scripts/Core/RoundCoordinator.cs` |
| Step playback | `Assets/Scripts/Core/Services/StepService.cs` |
| Scene entry (gameplay) | `Assets/Scripts/Core/GameBootstrap.cs` |
| Menu UI + start round | `Assets/Scripts/Menu/UI/MenuPresenter.cs` |
| Game states | `Assets/Scripts/Core/StateMachine/` |
| Server settings asset | `Assets/Scripts/Server/Data/ServerSettings.cs` (instance in `Resources/`) |
| Colyseus SDK | `Assets/Colyseus/` |
