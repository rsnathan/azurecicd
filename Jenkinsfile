#!/usr/bin/env groovy
pipeline {
    agent any
    options {
        timestamps()
        buildDiscarder(logRotator(daysToKeepStr: '7'))
    }
   
    environment {
        CODE_REPO_BRANCH = 'master'
        IMAGE_NAME = 'dotnetcore-helloworld'
        DOCKER_REGISTRY_URL = ""
    }

    stages {
        stage('Checkout Code') {
            steps{
                script{
                    dir('code-repo') {
                    echo "Checkout SCM"
                    deleteDir()
                        retry(3){
                        checkout scm
                        }
                    stash includes: 'dotnetcore-helloworld/**/*', name: 'dotnetcore-helloworld'
                    stash includes: 'Dockerfile', name: 'dockerfile'

                    }
                }
            }
        }
        stage('Test Code') {
            steps{
                script{
                   docker.image('microsoft/dotnet:2.1-sdk').inside('--dns=8.8.8.8'){
                       deleteDir()
                       unstash 'dotnetcore-helloworld'
                       sh "ls -ltrh"
                       dir('dotnetcore-helloworld'){
                           sh 'dotnet restore'
                           dir('UnitTests'){
                               sh 'dotnet test'
                           }
                       }

                    stash includes: 'dotnetcore-helloworld/**/*', name: 'dotnetcore-helloworld-build'

                   }
                }
            }
        }
        stage('Build Code') {
            steps{
                script{
                   docker.image('microsoft/dotnet:2.1-sdk').inside('--dns=8.8.8.8'){
                       deleteDir()
                       unstash 'dotnetcore-helloworld-build'
                       sh "ls -ltrh"
                       dir('dotnetcore-helloworld'){
                           dir('DotNetCoreHelloWorld'){
                               sh 'dotnet publish -c Release -o app'
                               stash includes: 'app/**/*', name: 'app'

                           }
                       }


                   }
                }
            }
        }
        stage('Build Docker Image'){
            steps{
                script{
                    dir('build-repo') {
                        deleteDir()
                        echo "Build docker image and push to Registery"
                        unstash 'app'
                        unstash 'dockerfile'
                        sh 'ls -ltrh'
                        dockerImageRepo = "rahulswaminathan066/${IMAGE_NAME}:${CODE_REPO_BRANCH}"
                        withDockerRegistry([ credentialsId: "docker-registry-credentials", url: '' ]){
                            dockerImage = docker.build(dockerImageRepo, "--pull .")
                            echo  "Push Image to Dockerhub"
                            dockerImage.push()
                            
                        }

                    }

                }
            }
        }
        stage('Deploy App to Azure') {
            steps{
                script{
                    withCredentials([azureServicePrincipal('principal-credentials-id')]) {
                        STAGING_ENV_CREATED=false
                        GROUP_LIST=execAureCommand('az group list --query "[?name==\'myResourceGroup\']"')
                        echo "GROUP_LIST ${GROUP_LIST}"
                        if (GROUP_LIST == '[]'){
                            execAureCommand('az group create --name myResourceGroup --location "West Europe"')
                        }
                        PLAN_LIST=execAureCommand('az appservice plan list --query "[?name==\'myAppServicePlan\']"')
                        echo "PLAN_LIST ${PLAN_LIST}"
                        if (PLAN_LIST == '[]'){ 
                        execAureCommand('az appservice plan create --name myAppServicePlan --resource-group myResourceGroup --sku S1 --is-linux')
                        }
                        WEBAPP_LIST = execAureCommand('az webapp list --resource-group myResourceGroup --query "[].{hostName: \'dotnetcore-helloworld\'}"')
                        echo "WEBAPP_LIST is ${WEBAPP_LIST}"
                        if (WEBAPP_LIST == '[]'){
                            echo "First Deployment will not deploy to Staging..!!"
                            execAureCommand("az webapp create --resource-group myResourceGroup --plan myAppServicePlan --name ${IMAGE_NAME} --deployment-container-image-name ${dockerImageRepo};az webapp config appsettings set --resource-group myResourceGroup --name ${IMAGE_NAME} --settings WEBSITES_PORT=80")
                            echo "Access you webapp here at http://${IMAGE_NAME}.azurewebsites.net"
                        }
                        else{
                            echo "Initiating Blue-Green Deployment.Deploying to staging first"
                            execAureCommand("az webapp deployment slot create --name dotnetcore-helloworld --resource-group myResourceGroup --slot staging --configuration-source dotnetcore-helloworld")
                            echo "Initiating Healthcheck of Staging app "
                            if(healthCheck('https://dotnetcore-helloworld-staging.azurewebsites.net/api/values/5','dotnetcore-helloworld-staging')){
                                echo "Healthcheck sucessfull initiating swap of staging <-> production"
                                execAureCommand("az webapp deployment slot swap  --name dotnetcore-helloworld --resource-group myResourceGroup --slot staging")
                                STAGING_ENV_CREATED=true
                            }
                            else{
                                echo "Healthcheck failed this release will not be deployed to production..!!"

                            }
                        }
                    }
                }
            }
        }
        stage('Initiating HealthCheck Production App') {
            steps{
                script{
                   if(healthCheck('https://dotnetcore-helloworld.azurewebsites.net/api/values/5','dotnetcore-helloworld')){
                        if(STAGING_ENV_CREATED){
                            echo "Deleting Staging Environment"
                            execAureCommand("az webapp deployment slot delete --name dotnetcore-helloworld --resource-group myResourceGroup --slot staging")
                        }
                       echo "Deployment Sucessfull access app at https://dotnetcore-helloworld.azurewebsites.net"
                   }
                   else{
                       echo "Health Check failed need to rollback....!!"
                   }
                }
            }
        }
    }
}
def execAureCommand(comandToExec){
    docker.image('microsoft/azure-cli').inside('--dns=8.8.8.8') {
        withCredentials([azureServicePrincipal('principal-credentials-id')]) {
            sh 'az login --service-principal -u $AZURE_CLIENT_ID -p $AZURE_CLIENT_SECRET -t $AZURE_TENANT_ID'
            sh 'az account set -s $AZURE_SUBSCRIPTION_ID'
            return sh(
                        script: comandToExec ,
                        returnStdout: true
            ).trim()
        }
    }
}
def healthCheck (String urlToWaitFor,nameService = '') {
  docker.image('tutum/curl').inside('--dns=8.8.8.8') {
    def responseCode
    timeout(time: 5, unit: 'MINUTES') {
        waitUntil {
        if(0 != sh(script: "curl -m 5 -skL ${urlToWaitFor} -o /dev/null", returnStatus: true)){
            sleep time: 3, unit: 'SECONDS'
            echo "${nameService} => ${urlToWaitFor} is not accessible, is started without errors?"
            return false
        }

        responseCode = sh(script: "curl -m 5 -skL -w \"%{http_code}\" ${urlToWaitFor} -o /dev/null || echo 'no-response'", returnStdout: true).trim()
        echo "${nameService} => ${urlToWaitFor} returned ${responseCode}"
        return (
            '200'.equals(responseCode)
        );
        }
        return true
    }
  }
}