# AGENTS.md

Guidelines for AI agents and contributors working in this repository.

## Project overview

Collection Manager is an osu! collection creation/editing/export tool (WinForms GUI + CLI). This repository is **CollectionManagerEnhanced**, a fork of [Piotrekol/CollectionManager](https://github.com/Piotrekol/CollectionManager). The fork adds the **merged .osz export** feature (pack many beatmaps into a single .osz) and related UX improvements. Keep the upstream base intact: only add, do not rewrite upstream behaviour unless asked.

## Prerequisites / toolchain

- **.NET SDK 9.x** is required (SDK 8 cannot build the `net9.0` targets). The classic WinForms projects target `net9.0-windows`.
- **`LangVersion=preview` is mandatory** — the code uses the C# 14 preview `field` keyword (`CollectionManager.Core/Types/OsuCollection.cs`). It is already set in `Directory.Build.props` and `CollectionManager.Core.csproj`; never revert it to `latest` (that breaks the build with CS8652).
- `Nullable` is **disabled**, `ImplicitUsings` is **disabled** (explicit `using` everywhere), `AnalysisMode=Recommended` (lots of legacy CA warnings — do not chase them, but avoid introducing new ones).

## Build & test

```bash
dotnet build CollectionManager.sln -c Debug   # 0 errors baseline
dotnet test CollectionManager.sln -c Debug --no-build   # all tests must pass (46 today)
dotnet run --project CollectionManager.App.WinForms     # run the GUI
```

CI (.github/workflows/ci.yml) runs on PRs to `master`; `release.yml` runs on tags.

## Solution layout & dependency direction

Order matters — a project may only reference projects listed below it:

| Project | References | Responsibility |
|---|---|---|
| `CollectionManager.Core` | *(none)* | Data models: `Beatmap`, `BeatmapExtension`, `LazerBeatmap`, `OsuCollection`, `MergedOszBeatmap`, `Enums/PlayMode`, file IO readers/writers, `Extensions/` (e.g. `CancellationTokenSourceExtensions`) |
| `CollectionManager.Common` | Core | **Interfaces + shared enums**: `Interfaces/Controls/*`, `Interfaces/Forms/*` (IForm, IMergedOszExportForm, …), `MainSidePanelActions`, `MessageBoxType` |
| `CollectionManager.Extensions` | Common | Utilities (`BeatmapUtils`) and exporter logic (`Modules/MergedOsz/MergedOszExporter`, `Modules/Exporter/BeatmapExporter`) |
| `CollectionManager.Audio` | Core | Audio playback (NAudio) |
| `CollectionManager.App.Shared` | Core, Extensions, Common, Audio | **Model + Presenter + side-panel action handlers** (MVP business layer, UI-framework agnostic) |
| `CollectionManager.WinForms` | Core, Extensions, Common, ObjectListView2012 | **View layer**: `Forms/*` (BaseForm, MergedOszExportForm, MainFormView, StartupForm), `Controls/*` (TabControlEx, CombinedBeatmapPreviewView, CollectionTextView, …), `FormServices` DI registration |
| `CollectionManager.App.WinForms` | App.Shared, Core, Extensions, Common, WinForms, Audio, ObjectListView | Exe bootstrap (`WinFormsInitalizer`) |
| `CollectionManager.App.Cli` | Core, Extensions | CLI (CommandLineParser), sub-commands convert/create/generate |

**Hard rule: `CollectionManager.WinForms` (and `App.Cli`) must never reference `CollectionManager.App.Shared`.** Views talk to presenters through interfaces defined in `CollectionManager.Common`. If a WinForms control is needed by a presenter (e.g. a preview panel), expose it as an interface property on the form interface (`IMergedOszExportForm.CombinedBeatmapPreviewView`), implemented by the WinForms side.

## Architecture conventions (MVP)

- **New feature = Model + Presenter + View interface + View**. Model/Presenter live in `App.Shared/Models`/`App.Shared/Presenters`; the view interface goes in `Common/Interfaces/Forms` (extending `IForm`); the WinForms view lives in `WinForms/Forms`, extends `BaseForm` and implements the interface with hand-wired events.
- **Form registration is automatic**: `FormServices.RegisterServices` scans the `CollectionManager.WinForms` assembly for classes implementing `IForm` and registers them transiently against their `CollectionManager`-namespace interfaces. Resolve with `Initalizer.GuiComponentsProvider.GetClassImplementing<IMyForm>()`.
- **Adding a top-menu feature**: ① add a value to `MainSidePanelActions` (Common) ② add the menu item in `MainSidePanelView.Designer.cs` + fire `SidePanelOperation` in `MainSidePanelView.cs` ③ create a handler in `App.Shared/Misc/SidePanelActions/*` implementing `IMainSidePanelActionHandler` (Action + HandleAsync) ④ register it in `SidePanelActionsHandler.CreateDefaultHandlers`.
- **Long-running work** (e.g. export): run on `Task.Run`, report via `IUserDialogs.CreateProgressFormAsync(stringProgress, percentageProgress)` + `CancellationTokenSource` aborted from `progressForm.AbortClicked` (pattern: `App.Shared/Presenters/Forms/MergedOszExportFormPresenter.ExportAsync`). Dialogs go through `IUserDialogs`, never `MessageBox` directly in App.Shared.
- **MVP wiring example**: `MergedOszExportHandler` (App.Shared) → `MergedOszExportForm` (WinForms) + `MergedOszExportFormPresenter` + `MergedOszExportModel`.

## Download architecture (mirrors, osu!collector import)

- `downloadSources.json` (next to the exe, copied from `App.Shared/downloadSources.json`) drives all download sources: official `osu!` (cookies, `OsuDownloader`) and `osu mirrors (anonymous)` (`MirrorDownloader`, `RequiresLogin=false`, ships 9 community mirrors). `DownloadSource.Mirrors` (list of `DownloadSourceMirror` with `{0}`-templates) is optional and backward compatible.
- `OsuDownloadManager.GetDownloadItem` turns the mirror list into `DownloadItem.Candidates` (order = priority); `DownloadManager.TrySwitchMirror` retries a failed item with the next mirror (requeues item + client) before marking it errored; `DownloadItem.Candidates`/`CurrentMirrorIndex` live in `Extensions/Modules/Downloader/Api/DownloadItem.cs`.
- Mirror order follows osu-collect's default (osu.direct → nerinyan → sayobot → nekoha → beatconnect → osudl → catboy.best → hinamizawa → nzbasic). Users can edit the JSON for custom mirrors.
- osu!collector import: `Extensions/Modules/API/OsuCollector/OsuCollectorApi.cs` (strict link/ID parsing + `GET https://osucollector.com/api/collections/{id}` + `ToOsuCollection(MapCacher)` — checksums/IDs make maps land in `DownloadableBeatmaps` so the existing download flow works); presenter `App.Shared/Presenters/Forms/OsuCollectorImportFormPresenter.cs`; window `WinForms/Forms/OsuCollectorImportForm.cs`. After-import behavior is a persisted setting `OsuCollectorImportDownloadBehavior` (`Ask`/`Yes`/`No`, see the Settings four-piece set: `Settings.settings`/`Settings.Designer.cs`/`IAppSettingsProvider`/`SettingsProvider`) with values in `Common/Interfaces/Forms/OsuCollectorImportBehaviors`.
- Adding a new user setting: update all four pieces (Settings.settings XML + generated Designer class + interface + provider) and keep the name in sync.

## Code style

- File-scoped namespaces for new files; legacy block namespaces survive in old WinForms designer files.
- Private fields `_camelCase`, public members `PascalCase`, C# events.
- Modern C# (preview LangVersion) is fine: raw string literals, collection expressions, tuples.
- UI strings are in **English** (matching the upstream UI).
- Put `using` directives at the very top of the file (WinForms designer files are old-style and deliberately have none).
- xUnit `Assert.*` style is used in tests (no FluentAssertions chains, despite AwesomeAssertions being referenced).

## Known pitfalls (from real bugs — read before touching these areas)

- `ZipArchive.AddEntry(file, entryPath)` is an **extension method** in the `SharpCompress.Archives` namespace — a `using SharpCompress.Archives;` is required in `MergedOszExporter.cs`.
- `CancellationTokenSource.TryCancel()` is a project extension (`CollectionManager.Core/Extensions`), needing `using CollectionManager.Core.Extensions`.
- WinForms `ListView.GetItemAt` only has an `(int x, int y)` overload (no Point overload); use `HitTest(Point)` to resolve sub-items.
- `ListView.InsertionMark` is invisible unless the ListView has a (1×1) `SmallImageList` — the merged-osz form sets one up for the reorder indicator.
- `BeatmapThumbnailPresenter` loads images on a `BackgroundWorker` and sets view data from that thread; the view setters use `Invoke`. When wiring a new preview, subscribe the presenter chain (`CombinedBeatmapPreviewPresenter` → `BeatmapThumbnailModel`/`MusicControlModel`) in the *presenter* constructor.
- **Interface event wiring is easy to lose**: when extending an IForm interface, every event must be declared on the form AND subscribed; a missing view-side subscription (e.g. `SelectedIndexChanged += …`) silently disables the feature. Verify both ends when wiring UI.
- `.osz` exports zip **flat** (no per-map folders), using one shared `{index}.{ext}` resource namespace per song.
- Tests that exercise the exporter build a fake osu! Songs directory and point `BeatmapUtils.OsuSongsDirectory` at it (see `Extensions.Tests/Modules/MergedOsz/MergedOszExporterTests.cs`).

## Git workflow

- Single `master` branch; commits directly on master (fork project, no PR flow in this repo).
- Remote: `origin = https://github.com/billpenn2005/CollectionManagerEnhanced.git` (the upstream read-only repo is available from git history).
- Commit messages: short imperative summaries, e.g. `Add merged osz export feature`, `Improve merged osz export window UX`.
- Before committing: `dotnet build` must be 0 errors and `dotnet test` fully green.