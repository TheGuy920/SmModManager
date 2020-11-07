// ==UserScript==
// @name         Alert Message Join [Steam Id]
// @version      1.0
// @description  get the user's ID as a file
// @author       theguy920#1402(discord)
// @match        https://steamcommunity.com/profiles/*
// @match        https://steamcommunity.com/id/*
// @match        https://steamcommunity.com/chat/*
// ==/UserScript==
setTimeout(function() {
        var currentUrl = window.location.href;
        if (!currentUrl.includes("chat")) {
            if (currentUrl.includes("friends")) {
                var x = [
                    document.getElementsByClassName("selectable friend_block_v2 persona in-game "),
                    document.getElementsByClassName("selectable friend_block_v2 persona online "),
                    document.getElementsByClassName("selectable friend_block_v2 persona offline ")
                ];
                for (var q = 0; q < x.length; q++) {
                    var p = x[q];
                    for (var g = 0; g < p.length; g++) {
                        var data = p[g];
                        var UserId = data.getAttribute("data-steamid");
                        var c = data.children;
                        for (var i = 0; i < c.length; i++) {
                            if (typeof c[i].href !== "undefined") {
                                c[i].href = "javascript:alert(".concat("\'JoinUser: ", UserId, "\')");
                            }
                        }
                    }
                }
            }
        }
    },
    500);

if (window.location.href.includes("steamworkshopdownloader")) {
    window.parent.postMessage(["steamdownloaderFrameLoaded", true], "*");
}