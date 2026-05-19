# GitHub Repository Setup

This document describes a simple, clean way to publish **SASD ScreenSaver Lab** as a GitHub repository.

## Recommended repository name

```text
SASD-ScreenSaverLab
```

This name is clear, specific, and still leaves room for future related projects such as a separate screensaver engine, visualizer library, or effect pack.

## Recommended short description

```text
Experimental C#/.NET screensaver and visualizer lab for original SASD-style animated effects.
```

## Recommended GitHub topics

```text
csharp
.net
dotnet
windows-forms
screensaver
visualizer
graphics
animation
sasd
```

## Initial local Git setup

Run these commands in the project root directory:

```bash
git init
git add .
git commit -m "Initialize SASD ScreenSaver Lab prototype"
git branch -M main
```

## Option A: Create the GitHub repository with GitHub CLI

If the GitHub CLI is installed and authenticated, this is the quickest option:

```bash
gh repo create sasdgmbh/SASD-ScreenSaverLab \
  --public \
  --source=. \
  --remote=origin \
  --push
```

If the repository should be created under the personal account instead of the SASD organization, use:

```bash
gh repo create Robin-Goerlach/SASD-ScreenSaverLab \
  --public \
  --source=. \
  --remote=origin \
  --push
```

## Option B: Create the GitHub repository in the browser

1. Create a new empty GitHub repository named `SASD-ScreenSaverLab`.
2. Do not initialize it with a README, license, or `.gitignore`, because these files already exist locally.
3. Add the remote and push:

```bash
git remote add origin https://github.com/sasdgmbh/SASD-ScreenSaverLab.git
git push -u origin main
```

For a personal repository, use:

```bash
git remote add origin https://github.com/Robin-Goerlach/SASD-ScreenSaverLab.git
git push -u origin main
```

## First recommended tag

After the initial version has been built and started successfully, create a small prototype tag:

```bash
git tag v0.1.2
git push origin v0.1.2
```

## Notes

The screenshot shown in the README is stored in:

```text
docs/screenshots/sasd-screensaverlab-star-drift.png
```

If a real screenshot from your own monitor looks better later, simply replace this file with the same filename. The README will continue to display it automatically.
