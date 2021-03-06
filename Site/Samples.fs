﻿namespace Site

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.Html.Client

/// Support code for the sample catalog.
[<JavaScript>]
module Samples =

    type Sample =
        private {
            FileName : string
            Id : string
            Keywords : list<string>
            Render : Dom.Element -> unit
            Title : string
        }

    let private ( ++ ) a b =
        match a with
        | Some _ -> a
        | None -> b

    let private req name f =
        match f with
        | None -> failwith ("Required property not set: " + name)
        | Some r -> r

    type Builder =
        private {
            mutable BFileName : option<string>
            mutable BId : option<string>
            mutable BKeywords : list<string>
            mutable BRender : option<Dom.Element -> unit>
            mutable BTitle : option<string>
        }

        member b.Create() =
            let id = req "Id" (b.BId ++ b.BTitle)
            let title = defaultArg (b.BTitle ++ b.BId) "Sample"
            {
                FileName = req "FileName" b.BFileName
                Id = id
                Keywords = b.BKeywords
                Render = req "Render" b.BRender
                Title = title
            }

        member b.FileName(x) = b.BFileName <- Some x; b
        member b.Id(x) = b.BId <- Some x; b
        member b.Keywords(x) = b.BKeywords <- x; b
        member b.Render(x) = b.BRender <- Some x; b
        member b.Title(x) = b.BTitle <- Some x; b

    let Build () =
        {
            BId = None
            BFileName = None
            BKeywords = []
            BRender = None
            BTitle = None
        }

    let private Clear (el: Dom.Element) =
        while el.HasChildNodes() do
            el.RemoveChild(el.FirstChild) |> ignore

    type Sample with

        member s.Show() =
            let sMain = JS.Document.GetElementById("sample-main")
            let sSide = JS.Document.GetElementById("sample-side")
            Clear sMain
            Clear sSide
            s.Render(sMain)
            let url = "http://github.com/intellifactory/websharper.d3/blob/master/Site/" + s.FileName
            let side =
                Div [
                    Div []
                    |>! OnAfterRender (fun self ->
                        match JS.Document.GetElementById(s.Id) with
                        | null -> ()
                        | el ->
                            let copy = el.CloneNode(true)
                            copy.Attributes.RemoveNamedItem("id") |> ignore
                            self.Append(copy))
                    A [Attr.Class "btn btn-primary btn-lg"; HRef url] -< [Text "Source"]
                ]
            side.AppendTo("sample-side")

    type Set =
        private
        | Set of list<Sample>

        static member Create(ss) = Set [for (Set xs) in ss do yield! xs]
        static member Singleton(s) = Set [s]

        member s.Show() =
            JQuery.JQuery.Of(fun () ->
                let (Set samples) = s
                let select (s: Sample) (dom: Dom.Element) =
                    let j = JQuery.Of("#sample-navs ul").Children("li").RemoveClass("active")
                    JQuery.Of(dom).AddClass("active").Ignore
                    s.Show()
                let rec navs =
                    UL [Attr.Class "nav nav-pills"] -< (
                        samples
                        |> List.mapi (fun i s ->
                            LI [A [HRef "#"] -< [Text s.Title]]
                            |>! OnAfterRender (fun self -> if i = 0 then select s self.Dom)
                            |>! OnClick (fun self _ -> select s self.Dom))
                    )
                navs.AppendTo("sample-navs"))
