pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/core/sdk:3.1'
        }
    }
    environment {
        DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
    }
    stages {
        stage('Restore') {
            steps {
                sh "dotnet restore"
            }
        }
        stage('Clean') {
            steps {
                sh 'dotnet clean'
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build'
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test Rambler.Test/Rambler.Test.csproj'
            }
        }

    }
}