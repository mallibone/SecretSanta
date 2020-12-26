// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.IO
open Twilio
open Twilio.Rest.Api.V2010.Account
open Microsoft.Extensions.Configuration

type Person = { Name : string; MobileNumber : string }

let configuration = 
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .Build()

let initTwilio =
    let accountSid = configuration.GetSection("Twilio").["Sid"]
    let authToken = configuration.GetSection("Twilio").["AuthToken"]
    TwilioClient.Init(accountSid, authToken)

let parseToPerson (inputString:string) =
    let csvItems = inputString.Split(";")
    {Name = csvItems.[0]; MobileNumber = csvItems.[1]}

let rec informSecretSantas dryRun (santas: (Person * Person) list) =
    match santas with
    | [] -> ()
    | head::tail ->
        let santaName = (fst head).Name
        let giftReceiver = (snd head).Name
        let phoneNumber = (fst head).MobileNumber
        let body = $"Im {(DateTime.UtcNow.AddYears(1).Year)} bist du der Wichtel von: {giftReceiver}\r\n\r\nUnd denk daran {santaName}, dass das Geschenk nicht mehr als 25.- kosten sollte. 😉"
        if dryRun then
            printfn "%s" body
            printfn "Sending to %s" phoneNumber
        else
            let msg = MessageResource.Create(Twilio.Types.PhoneNumber(phoneNumber), body=body, from=Twilio.Types.PhoneNumber("+12569803879"))
            printfn "%A" msg.Status
        informSecretSantas dryRun tail

let random = Random()
let rec assignSecretSantas (people : Person seq) =
    let secretSantasAssignments =
        people
        |> Seq.sortBy(fun x -> random.Next())
    
    let assignments = 
        secretSantasAssignments
        |> Seq.zip people
        |> Seq.toList
    
    if assignments |> Seq.exists (fun a -> fst a = snd a) then
        assignSecretSantas people
    else
        assignments

// Define a function to construct a message to print
let from whom =
    sprintf "from %s" whom

[<EntryPoint>]
let main argv =

    initTwilio 

    File.ReadAllLines("SecretSantaList.csv")
    |> Array.map parseToPerson
    |> assignSecretSantas
    |> informSecretSantas true

    0 // return an integer exit code