# 🔧 Fix Publish Profile for GitHub Actions

## The Problem
The deployment is failing with "Publish profile is invalid" error.

## The Solution
Use the exact publish profile XML from the `fresh-publish-profile.xml` file.

## Step-by-Step Fix

### 1. Copy the Exact XML
Open the file `fresh-publish-profile.xml` in your project folder and copy this EXACT content:

```
<publishData><publishProfile profileName="sentiment-analysis-ryan-20241215 - Web Deploy" publishMethod="MSDeploy" publishUrl="sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.scm.eastus-01.azurewebsites.net:443" msdeploySite="sentiment-analysis-ryan-20241215" userName="REDACTED" userPWD="REDACTED" destinationAppUrl="https://sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.eastus-01.azurewebsites.net" SQLServerDBConnectionString="REDACTED" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="https://portal.azure.com" webSystem="WebSites"><databases /></publishProfile><publishProfile profileName="sentiment-analysis-ryan-20241215 - FTP" publishMethod="FTP" publishUrl="ftps://waws-prod-blu-191.ftp.azurewebsites.windows.net/site/wwwroot" ftpPassiveMode="True" userName="REDACTED" userPWD="REDACTED" destinationAppUrl="https://sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.eastus-01.azurewebsites.net" SQLServerDBConnectionString="REDACTED" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="https://portal.azure.com" webSystem="WebSites"><databases /></publishProfile><publishProfile profileName="sentiment-analysis-ryan-20241215 - Zip Deploy" publishMethod="ZipDeploy" publishUrl="sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.scm.eastus-01.azurewebsites.net:443" userName="REDACTED" userPWD="REDACTED" destinationAppUrl="https://sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by.eastus-01.azurewebsites.net" SQLServerDBConnectionString="REDACTED" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="https://portal.azure.com" webSystem="WebSites"><databases /></publishProfile></publishData>
```

### 2. Update GitHub Secret
1. Go to: https://github.com/ryanvarghese/sentiment-analysis-app/settings/secrets/actions
2. Find `AZUREAPPSERVICE_PUBLISHPROFILE` secret
3. Click **"Update"**
4. **Delete the old content completely**
5. **Paste the new XML exactly as shown above**
6. Click **"Update secret"**

### 3. Revert Workflow to Use Publish Profile
The workflow needs to use the publish profile method, not Azure login.

### 4. Test Deployment
1. Go to: https://github.com/ryanvarghese/sentiment-analysis-app/actions
2. Click **"Re-run all jobs"** on the latest workflow
3. Watch it deploy successfully!

## Important Notes
- Make sure there are NO extra spaces or characters
- The XML must be on ONE line (no line breaks)
- Copy exactly as shown above
- The app name in the workflow is now correct: `sentiment-analysis-ryan-20241215-ezgkezdjfsdpf8by`
