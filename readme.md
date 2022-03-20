Prerequisites:
* Must install .net 6 SDK or runtime first.
* No param file yet, but will add. Modify class variable definitions for now.
* must have domain using Azure name servers.  
* Must have Azure Public DNS Zone named after domain
* Must create app registration. no API permissions required
* Grant app principal (app registration) record set contributor role to Azure Public DNS zone using RBAC

To Deploy: 
1. Create user that the crontab will run under. In this example cron-azure-ddns. This saves in separate user home directory to protect app secret
    1. Create user with home directory
        `sudo useradd -m [username]`
    2. Set password
        `sudo passwd [username]`
    3. (optional) If you have a user group for all chron users, add to group
        `sudo usermod -a -G [groupname] [username]`

2. Create appsecret.txt in root of project directory with only the app secret from app registration
3. In Visual Studio Code terminal run publish command (replace):

`sudo dotnet publish -c Release -o /home/cron-azure-ddns/bin/azure-ddns -r linux-x64`

3. create cron tab
    1. Impersonate the user created in step 1:
    `sudo -u [username] -s`

    2. Edit crontab:
    `crontab -e`

    3. Add line cron line. This example runs every minute and logs to home directory:
    `* * * * * dotnet /home/cron-azure-ddns/bin/azure-ddns/azure-ddns.dll >> /home/cron-azureddns/CronLog/cron-azure-ddns.log 2>&1 `

4. Check cron run log
    `cat /var/log/syslog | grep cron`

    