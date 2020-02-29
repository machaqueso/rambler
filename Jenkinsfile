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
        stage('Test') {
            steps {
                sh 'dotnet test --logger "trx;LogFileName=test_results.xml"'
                step([$class: 'MSTestPublisher', testResultsFile:"**/test_results.xml", failOnError: true, keepLongStdio: true])
            }
        }

    }
}