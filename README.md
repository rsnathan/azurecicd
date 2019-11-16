# azurecicd

Implement Jenkins Pipeline to Deploy to Azure Cloud Webapp app Containers with Blue Green Deployment


![alt text] (https://github.com/rsnathan/azurecicd/blob/master/jenkins.png)


We use Azure Cloud Webapp app Containers https://azure.microsoft.com/en-us/services/app-service/containers/ to run containers in Azure

We make of deployment slots https://docs.microsoft.com/en-us/azure/app-service/web-sites-staged-publishing for Blue Green Deployment  

We curl the endpoint(https://dotnetcore-helloworld.azurewebsites.net/api/values/5) to check the health of our container and use swap feature of deployment slots to achieve Blue Green Deployment
