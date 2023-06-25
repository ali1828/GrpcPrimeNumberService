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


