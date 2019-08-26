namespace AsteriskConfigParser

open System

module public AsteriskDialplanObjects =
    type ExtensionAction = {Priority: string;
                            App: string;
                            AppParams: string}

    type ExtensionOneLine = {Name: string;
                             ContextName: string;
                             Action: ExtensionAction;}

    type Extension = {Name: string;
                      ContextName: string;
                      Actions: ExtensionAction list;
                      ContextExtenLinks: (string*string) list}

    type Context = {Name: string;
                    Extensions: Extension list}

    let GetExtensionAction priority (actionString: string) =
        let fstBracketIdx = actionString.IndexOf("(")
        let lstBracketIdx = actionString.LastIndexOf(")")
        let app, appParams =
            match fstBracketIdx, lstBracketIdx with
            | -1, _ -> actionString,""
            | _ , -1 ->
                (actionString.[0..(fstBracketIdx - 1)],
                 actionString.[(fstBracketIdx + 1)..])
            | _, _ ->
                (actionString.[0..(fstBracketIdx - 1)],
                 actionString.[(fstBracketIdx + 1)..(lstBracketIdx - 1)])

        {Priority = priority; App = app; AppParams = appParams}

    let StringJoin (separator: string) (stringList: seq<string>) =
        String.Join(separator, stringList)

    let GetExtensionString  (extenConfLine: string)  =
        match extenConfLine.Split("=>") with
        | spltd when spltd.Length = 2 -> spltd.[1].Trim()
        | spltd when spltd.Length = 1 -> spltd.[0].Trim()
        | spltd when spltd.Length > 2 ->
            //log to loger of strange situation
            printfn "Got strange exten line - %s" extenConfLine
            (spltd.[1..] |> StringJoin " - ").Trim()
        | _ -> extenConfLine.Trim()

    let GetExtensionOneLine (extenLine: string) contextName =
        let extenData = GetExtensionString extenLine
        match extenData.Split(",") with
        | [|extName; priority; app|] ->
                {Name = extName; ContextName = contextName; Action = (GetExtensionAction priority app)}
        | arr when arr.Length > 3 ->
                {Name = arr.[0]; ContextName = contextName; Action = (GetExtensionAction arr.[1] (arr.[2..] |> StringJoin ","))}
        | _ -> {Name = ""; ContextName = "";
                Action = { Priority = "";App = "";AppParams = ""}}


    let rec PreprocessExtensionLines (lines: string list) contextName preprocessed =
        match lines with
        | [] -> preprocessed
        | head::tail ->
            match head.Trim() with
            | empty when empty.Length = 0 ->  PreprocessExtensionLines tail contextName preprocessed
            | ctx when ctx.[0] = '['  ->
                    PreprocessExtensionLines tail (head.Trim().Trim([|'['; ']'|])) preprocessed
            | exten when exten.Length > 4 && exten.Substring(0,5) = "exten" ->
                    PreprocessExtensionLines tail contextName (
                        (GetExtensionOneLine exten contextName)::preprocessed)
            | _ -> PreprocessExtensionLines tail contextName preprocessed


    let GetContextLinkFromGoto (paramString: string) =
        let splited = paramString.Split(",")
        match splited with
        | [|context|] -> context ,""
        | [|context; exten; _|] when splited.Length > 2 ->  context,exten.Trim()
        | _ -> "",""

    let GetContextLinkFromGotoIf (paramString: string) =
        match paramString.IndexOf("?") with
        | -1 -> ["",""]
        | idx when idx > -1 ->
            paramString.[idx+1..].Split(":")
            |> Array.toList
            |> List.map( fun variant -> GetContextLinkFromGoto variant)
        | _ -> ["",""]

    let GetContextLink (app: string)  paramString =
        match app.ToLower() with
        | "goto"   -> [GetContextLinkFromGoto paramString]
        | "gotoif" -> GetContextLinkFromGotoIf paramString
        | _        -> []

    let GetExtenStionsFromOneLines (oneLines: ExtensionOneLine list) =
        oneLines
        |> List.groupBy( fun x -> x.ContextName, x.Name)
        |> List.map( fun grp ->
                        let (context, extension), oneLines = grp
                        let actions =
                            oneLines
                            |> List.map(fun ol ->
                                {Priority = ol.Action.Priority;
                                 App = ol.Action.App;
                                 AppParams = ol.Action.AppParams})
                            |> List.sortBy(fun act -> act.Priority)
                        let ctxLinks =
                            actions
                            |> List.collect(
                                fun act -> GetContextLink act.App act.AppParams)
                        {Name = extension;
                         ContextName = context;
                         Actions = actions;
                         ContextExtenLinks = ctxLinks
                        } )


    let GetContextsFromExtensions extensions =
        extensions
        |> List.groupBy (fun exten -> exten.ContextName)
        |> List.map (fun grp ->
                        let ctxName, extens = grp
                        {Name = ctxName; Extensions = extens})


    // let MakeContext ContextFirstLine lines =
    //     let ctxName = ContextFirstLine.Trim([|'[', ']'|])
    //     let extensions = LinesToExtension lines contefedoraxt