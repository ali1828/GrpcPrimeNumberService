# GrpcPrimeNumberService
# Implementation of gRPC Bi-Directional for finding prime numbers (Server + Clinet)
gRPC trasmits data in binary form that's why it is very fast.
## There are 4 types of gRPC procedure calls
1. Unary (1-1)
2. Client streaming
3. Server streaming
4. Bi-Directional streaming

This solution is implemented using Bi-Directional gRPC procedure call for sending and receving stream of data
from Client and Server respectively
This solution has 2 project GrpcPrimeNumbedrService and ConsoleApp acting as Client.

## To Change the Server serving IP
Go to GrpcPrimeNumberService project navigate to Properties folder -> launchSettings.json
For key applicationUrl you can assign a different port number as per your needs.

## Client is using hardcoded 10000 request each second
To change this you can go to Program.cs from the ConsoleApp and change the 10000 to whatever limit.

## Server is showing Top 10 max valid prime numbers as per the weights
You can change from top 10 to whatever number you want. On Server side code
goto Service -> PrimeNumberService.cs under DisplayRecordsPrimeNumbersPeriodically()
you can set Take(PlaceHolder) to any number you want.

## To extend the implementation 
you can create as many new proto files as you want in the Protos folder under the Server side impolementation.
To use these proto files from client side remember to refer these files in the csproj of client side

## Reference Images
![image](https://github.com/ali1828/GrpcPrimeNumberService/assets/15609965/6e8dda38-129c-4fbf-9f30-b3668e0728fd)
![image](https://github.com/ali1828/GrpcPrimeNumberService/assets/15609965/d38acc25-b663-4b5f-a8f9-c50e15cee7b2)

## Note:
I have added bat files for project server + client. Just execute those files and binaries will be generated respectively. 
If you are unable to execute those bat files then follow below steps:
1. Open cmd and navigate to root directory of the project GrpcPrimeNumberService
2. Write dotnet run and press enter
3. This command will starting serving the service
4. For client code, go to client code root directory ConsoleApp
5. Write dotnet run and press enter
6. It will the client and trying to send messages to the server. 
![image](https://github.com/ali1828/GrpcPrimeNumberService/assets/15609965/5122ef39-fa14-421d-b6fd-430af4d92f7a)

