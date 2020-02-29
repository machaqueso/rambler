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
        stage('Build') {
            steps {
                sh 'dotnet clean'
                sh "dotnet restore"
                sh 'dotnet build'
            }
        }
    }
}