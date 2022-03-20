About:
This is a .NET 6 Console Application that can be deployed on any platform. It is a DIY Dynamic DNS (DDNS) implementation using Azure Public DNS Zone. Schedule the published console app using Task Scheduler (Windows) or CronTab (Linux) at short intervals to quickly update DNS in case of IP change. Great for hosting your own services with a dynamics IP address at home, without having to pay for a pricey static IP. Host a game server for friends, host a portfolio website, or anything else! Take necessary security precautions to protect your server and network. 

Prerequisites:
* Must install .NET 6 SDK or runtime (see [https://dotnet.microsoft.com/en-us/download/dotnet/6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)).
* No param file yet, but will add in future. Modify class variable definitions manually with your own information for now: ClientID, TenantID, SubscriptionID, ResourceGroupName, DnsZoneName
* Must have custom domain from any public registrar. Set registrar to use Azure name servers (name servers can be found in Azure Public DNS Zone overview page)
* Must have Azure Public DNS Zone with the name set as the the domain name
* Must create Azure app registration. No API permissions required.
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

    