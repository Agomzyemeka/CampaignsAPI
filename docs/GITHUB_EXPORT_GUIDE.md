# 🚀 GitHub Export Guide

This guide will walk you through pushing your Campaigns API project to GitHub step by step.

---

## Prerequisites

1. **Git installed** on your machine
2. **GitHub account** at https://github.com
3. **Git configured** with your credentials

### Configure Git (if not already done)

```bash
# Set your username
git config --global user.name "Your Name"

# Set your email (use the email associated with your GitHub account)
git config --global user.email "your.email@example.com"
```

---

## Step 1: Create a GitHub Repository

1. Go to https://github.com
2. Click the **+** icon in the top right → **New repository**
3. Fill in the details:
   - **Repository name**: `CampaignsAPI`
   - **Description**: `A production-ready RESTful API for campaign management built with ASP.NET Core 6.0`
   - **Visibility**: Public (recommended for portfolio) or Private
   - **DO NOT** initialize with README (we already have one)
   - **DO NOT** add .gitignore (we'll create our own)
4. Click **Create repository**

---

## Step 2: Create .gitignore

Before committing, create a `.gitignore` file to exclude unnecessary files:

```bash
# Navigate to your project folder
cd "c:\Users\AGOMOH\Desktop\CODE\ASP.NET PROJECT\CampaignsAPI"
```

Create a `.gitignore` file with this content (already created):

```gitignore
# Build outputs
bin/
obj/
out/

# IDE
.vs/
.vscode/
*.user
*.suo
*.userosscache

# NuGet
packages/
*.nupkg

# Database files (generated at runtime)
*.db
*.db-journal

# User-specific
appsettings.Development.json
secrets.json

# Logs
*.log
logs/

# OS files
.DS_Store
Thumbs.db

# Temp
*.tmp
tmp/
temp/
```

---

## Step 3: Initialize Git Repository

Open a terminal in your project folder:

```powershell
# Navigate to project folder
cd "c:\Users\AGOMOH\Desktop\CODE\ASP.NET PROJECT\CampaignsAPI"

# Initialize git repository
git init

# Add all files
git add .

# Create initial commit
git commit -m "Initial commit: Campaigns API with JWT auth, EF Core, and Docker support"
```

---

## Step 4: Connect to GitHub

```powershell
# Add GitHub as remote origin (replace with YOUR repository URL)
git remote add origin https://github.com/YOUR_USERNAME/CampaignsAPI.git

# Rename branch to main (GitHub default)
git branch -M main

# Push to GitHub
git push -u origin main
```

### If using SSH instead of HTTPS:

```powershell
git remote add origin git@github.com:YOUR_USERNAME/CampaignsAPI.git
git push -u origin main
```

---

## Step 5: Verify Upload

1. Go to your GitHub repository URL
2. Verify all files are uploaded
3. Check that README.md displays correctly

---

## Step 6: Add Repository Topics (Recommended)

Go to your repository settings and add topics for discoverability:

- `aspnet-core`
- `dotnet`
- `csharp`
- `rest-api`
- `entity-framework`
- `jwt-authentication`
- `docker`
- `swagger`

---

## Step 7: Update Repository Details

1. Click the ⚙️ gear icon next to "About"
2. Add a website URL (if deployed)
3. Add topics
4. Check "Releases" and "Packages" if applicable

---

## Making Future Changes

After making changes to your code:

```powershell
# Stage changes
git add .

# Commit with descriptive message
git commit -m "Add feature: campaign statistics endpoint"

# Push to GitHub
git push
```

### Good Commit Message Examples:

```
feat: add pagination to campaigns endpoint
fix: correct date validation in campaign creation
docs: update README with API examples
refactor: extract token generation to service
style: format code according to C# conventions
test: add unit tests for AuthService
```

---

## Setting Up GitHub Actions (CI/CD) - Optional

Create `.github/workflows/dotnet.yml`:

```yaml
name: .NET Build and Test

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 6.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

---

## Troubleshooting

### "fatal: remote origin already exists"

```powershell
git remote remove origin
git remote add origin https://github.com/YOUR_USERNAME/CampaignsAPI.git
```

### Authentication Failed

1. **HTTPS**: Use a Personal Access Token instead of password
   - Go to GitHub → Settings → Developer settings → Personal access tokens
   - Generate new token with `repo` scope
   - Use this token as your password

2. **SSH**: Set up SSH keys
   - Generate key: `ssh-keygen -t ed25519 -C "your.email@example.com"`
   - Add to GitHub: Settings → SSH and GPG keys → New SSH key

### "Updates were rejected because the remote contains work"

```powershell
git pull origin main --rebase
git push origin main
```

---

## Quick Commands Reference

```powershell
# Check status
git status

# View commit history
git log --oneline

# Create and switch to new branch
git checkout -b feature/new-feature

# Switch branches
git checkout main

# Merge branch into main
git checkout main
git merge feature/new-feature

# Delete branch
git branch -d feature/new-feature

# Undo last commit (keep changes)
git reset --soft HEAD~1

# Discard all local changes
git checkout -- .
```

---

## Your Final GitHub URL

After completing these steps, your project will be available at:

```
https://github.com/YOUR_USERNAME/CampaignsAPI
```

Share this URL in your:
- Resume
- LinkedIn profile
- Portfolio website
- Job applications

---

## Professional Tips

1. **Write good READMEs**: First impression matters
2. **Use meaningful commit messages**: Shows professionalism
3. **Keep sensitive data out**: Never commit secrets, use environment variables
4. **Add a LICENSE**: MIT is common for portfolios
5. **Pin the repository**: On your GitHub profile
6. **Add screenshots**: In your README for visual appeal

Good luck with your GitHub portfolio! 🎉
