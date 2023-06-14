console.log(data)
function reloadReplies() {
    comments.forEach(c => {
        if (c.ThreadID != null) {
            let parentDiv = document.getElementById(c.ThreadID)
            parentDiv.getElementsByClassName('replies_btn')[0].style.display = "block"

            let newCommentContainer = document.createElement('div')
            newCommentContainer.className = "comment_container"
            newCommentContainer.id = c.ChatID
            newCommentContainer.dataset.parent = c.ThreadID

            let liked = c.Liked ? "liked" : ""
            let disliked = c.Disliked ? "disliked" : ""

            newCommentContainer.innerHTML = "<div class='comment_card'>" +
                "<p><b>" + c.First + " " + c.Last + "</b></p>" +
                "<p>" + c.Message + "</p>" +
                "<div class='row'>" +
                "<div class='col - 6 likesdislikes'>" +
                "<text class='icon - foo fa thumbs_up" + liked + "'onclick='like_comment(this)' font-family='Font Awesome'>&#xf164</text>" +
                "<text class='icon - foo fa thumbs_down" + disliked + "'onclick='dislike_comment(this)' font-family='Font Awesome''>&#xf165</text>" +
                "</div>" +
                "<div class='comment_card_footer col - 6'>" +
                "<div class='pntr'>Likes " + c.Likes + "</div>" +
                "<div class='pntr'>Dislikes " + c.Dislikes + "</div>" +
                "<div onclick='show_replies(this)' class='replies_btn pntr' style='display: none'>Replies</div>" +
                "<div class='pntr' onclick='add_reply(this)'>Reply</div>" +
                "</div>" +
                "</div>" +
                "</div>"

            parentDiv.appendChild(newCommentContainer)
        }
        else if (c.threadID != null) {
            let parentDiv = document.getElementById(c.threadID)
            parentDiv.getElementsByClassName('replies_btn')[0].style.display = "block"

            let newCommentContainer = document.createElement('div')
            newCommentContainer.className = "comment_container"
            newCommentContainer.id = c.chatID
            newCommentContainer.dataset.parent = c.threadID

            let liked = c.liked ? "liked" : ""
            let disliked = c.disliked ? "disliked" : ""

            newCommentContainer.innerHTML = "<div class='comment_card'>" +
                "<p><b>" + c.first + " " + c.last + "</b></p>" +
                "<p>" + c.message + "</p>" +
                "<div class='row'>" +
                "<div class='col - 6 likesdislikes'>" +
                "<text class='icon - foo fa thumbs_up " + liked + "' onclick='like_comment(this)' font-family='Font Awesome'>&#xf164</text>" +
                "<text class='icon - foo fa thumbs_down " + disliked + "' onclick='dislike_comment(this)' font-family='Font Awesome''>&#xf165</text>" +
                "</div>" +
                "<div class='comment_card_footer col - 6'>" +
                "<div class='pntr'>Likes " + c.likes + "</div>" +
                "<div class='pntr'>Dislikes " + c.dislikes + "</div>" +
                "<div onclick='show_replies(this)' class='replies_btn pntr' style='display: none'>Replies</div>" +
                "<div class='pntr' onclick='add_reply(this)'>Reply</div>" +
                "</div>" +
                "</div>" +
                "</div>"

            parentDiv.appendChild(newCommentContainer)
        }
    })
}
const showContainers = document.querySelectorAll('.show_replies')

function show_replies(e) {
    let t = e.offsetParent
    let id = e.offsetParent.offsetParent.id
    let parentContainer = document.getElementById(id)
    //let parentContainer = e.target.closest('.comment_container')
    //let id = parentContainer.id

    if (id) {
        let childrenContainers = parentContainer.querySelectorAll('[data-parent = "' + id + '"]')
        childrenContainers.forEach(child => child.classList.toggle("opened"))

        //let childrenContainers = parentContainer.querySelectorAll('div.comment_container > div')
        //childrenContainers.forEach(child => child.classList.toggle("opened"))
    }
}



const addReply = document.querySelectorAll('.add_reply')

function add_reply(e) {
    let id = e.offsetParent.offsetParent.id
    let parentContainer = document.getElementById(id)

    console.log(id)
    if (id) {
        let container = document.getElementById(id)

        let innerDiv = document.createElement('div')
        innerDiv.className = "reply_card"
        innerDiv.id = "reply"
        innerDiv.innerHTML = "<textarea class='reply_text_area'></textarea><br><div class='reply_card_footer'><input type='submit' value='cancel' onclick='cancelReply(this)'><input type='submit' value='submit'onclick='submitReply(this)'></div>"

        container.appendChild(innerDiv)
    }
}


function cancelReply(e) {
    e.closest('.reply_card').remove()
    console.log(e.id)
}

function submitReply(e) {
    let replyContainer = e.closest('.reply_card')

    let threadID = null
    try {
        threadID = e.closest('.comment_container').id
    }
    catch (error) {
        console.log(error)
    }

    let postData = {
        mid: data.matchesInfoRecord.MatchId,
        accountid: accountID,
        message: replyContainer.getElementsByClassName('reply_text_area')[0].value,
        timestamp: new Date(),
        threadid: threadID
    }

    replyContainer.getElementsByClassName('reply_text_area')[0].value = "";


    $.ajax({
        type: "POST",
        url: "/Match/PostNewComment",
        data: postData,
        success: function (data) {
            getComments();
        },
        complete: function () {


            $('#chat').load(location.href + ' #comment_section', function (status) {
                getComments()
            }
            )
        },
        fail: function () {
        }
    });
}

function getComments() {

    console.log("mid: " + data.matchesInfoRecord.MatchId)

    let getData = {
        mid: data.matchesInfoRecord.MatchId
    }

    $.ajax({
        type: "GET",
        url: "/Match/GetComments",
        data: getData,
        success: function (data) {
            comments = data
        },
        complete: function () {
            reloadReplies()
        },
        fail: function () {
        }
    });
}

function like_comment(e) {

    let parentDiv = e.offsetParent;

    let thumbsDown = parentDiv.getElementsByClassName("thumbs_down")

    let postData = {
        chatID: e.offsetParent.offsetParent.id,
    }

    if (thumbsDown[0].classList.contains("disliked")){
        $.ajax({
            type: "POST",
            url: "/Match/UndislikeComment",
            data: postData,
            success: function (data) {
                getComments();
            },
            complete: function () {
                $('#chat').load(location.href + ' #comment_section', function (status) {
                    getComments()
                }
                )
            },
            fail: function () {
            }
        });
    }

    if (e.classList.contains("liked")) {
        $.ajax({
            type: "POST",
            url: "/Match/UnlikeComment",
            data: postData,
            success: function (data) {
                getComments();
            },
            complete: function () {
                $('#chat').load(location.href + ' #comment_section', function (status) {
                    getComments()
                }
                )
            },
            fail: function () {
            }
        });
    }
    else {
        $.ajax({
            type: "POST",
            url: "/Match/LikeComment",
            data: postData,
            success: function (data) {
                getComments();
            },
            complete: function () {
                $('#chat').load(location.href + ' #comment_section', function (status) {
                    getComments()
                }
                )
            },
            fail: function () {
            }
        });
    }
}


function dislike_comment(e) {
    let parentDiv = e.offsetParent;

    let thumbsUp = parentDiv.getElementsByClassName("thumbs_up")

    let postData = {
        chatID: e.offsetParent.offsetParent.id,
    }

    if (thumbsUp[0].classList.contains("liked")) {
        $.ajax({
            type: "POST",
            url: "/Match/UnlikeComment",
            data: postData,
            success: function (data) {
                getComments();
            },
            complete: function () {
                $('#chat').load(location.href + ' #comment_section', function (status) {
                    getComments()
                }
                )
            },
            fail: function () {
            }
        });
    }

    if (e.classList.contains("disliked")) {
        $.ajax({
            type: "POST",
            url: "/Match/UndislikeComment",
            data: postData,
            success: function (data) {
                getComments();
            },
            complete: function () {
                $('#chat').load(location.href + ' #comment_section', function (status) {
                    getComments()
                }
                )
            },
            fail: function () {
            }
        });
    }
    else {
        $.ajax({
            type: "POST",
            url: "/Match/DislikeComment",
            data: postData,
            success: function (data) {
                getComments();
            },
            complete: function () {
                $('#chat').load(location.href + ' #comment_section', function (status) {
                    getComments()
                }
                )
            },
            fail: function () {
            }
        });
    }
}

let status = data.matchesInfoRecord.Status;
if (status != "NS") {
    drawPlayerStatsSorting();
    drawTimeline();
    drawTeamCompare();
}
if (data.homeLineups != null) {
    drawLineups();
}

fixImages();


console.log(data);



function hover(d) {
    var allPlayers = data.allPlayerStatistics
    var ID = d.id
    var player = allPlayers.find(d => d.PID == ID)
    var xplayer = data.allxPlayerStatistics.find(d => d.PID == ID)
    var tooltip = document.getElementById("tooltip")
    var x = window.event.clientX + 100;
    var y = window.event.clientY + 300;

    let tooltips = d3.select("#player-stats").selectAll(".player_tooltips")
    tooltips.style("visibility", "hidden");

    d3.select("#outside-div").style("filter", "blur(2px)");

    tooltip.style.visibility = "visible";

    let playerData = allPlayers.find(x => x.PID == player.PID);


    var html = "";

    if (xplayer != undefined) {
        html =
            "<div class='text-center'>" +
            "<p id=\"tooltip-player-name\" style=\"position: relative; width: 410px; height: 90px; background-color: var(--gray); border: solid black; \"><img class=\"top-performer-image\" width = \"75\" height = \"75\" src =" + playerData.Headshot + " style=\" transform: translate(0px, 5px); \"> " + player.First + " " + player.Last + "</p > " +
            "<table id=\"tooltip-table\" class='table table-scroll table_style' style=\"width: 410px;table-layout: fixed; \">" +
            "<thead><tr><th>Stat</th><th>Actual</th><th>Expected</th></tr></thead>" +
            "<tbody style=\"max-height: 320px; overflow-y: scroll ;display: inline-block;width: 404px; \">" +
            "<tr><td>Rating</td><td>" + player.Rating.toFixed(1) + "</td><td>(" + xplayer.xRating.toFixed(1) + ")</td></tr>" +
            "<tr><td>Goals</td><td>" + player.Goals + "</td><td>(" + xplayer.xGoals.toFixed(1) + ")</td></tr>" +
            "<tr><td>Assists</td><td>" + player.Assists + "</td><td>(" + xplayer.xAssists.toFixed(1) + ")</td></tr>" +
            "<tr><td>Passes</td><td>" + player.Passes + "</td><td></td></tr>" +
            "<tr><td>Minutes</td><td>" + player.Minutes + "</td><td></td></tr>" +
            "<tr><td>Shots</td><td>" + player.Shots + "</td><td>(" + Math.abs(xplayer.xShots.toFixed(1)) + ")</td></tr>" +
            "<tr><td>Shots on Goal</td><td>" + player.Shots_on_goal + "</td><td></td></tr>" +
            "<tr><td>Saves</td><td>" + player.Saves + "</td><td>(" + xplayer.xSaves.toFixed(1) + ")</td></tr>" +
            "<tr><td>Passes</td><td>" + player.Passes + "</td><td></td></tr>" +
            "<tr><td>Key Passes</td><td>" + player.Key_passes + "</td><td>(" + xplayer.xAssists.toFixed(1) + ")</td></tr>" +
            "<tr><td>Accurate Passes</td><td>" + player.Passes_accurate + "</td><td></td></tr>" +
            "<tr><td>Pass %</td><td>" + (player.Pass_pct.toFixed(1) * 100) + "%</td><td></td></tr>" +
            "<tr><td>Tackles</td><td>" + player.Tackles + "</td><td></td></tr>" +
            "<tr><td>Blocks</td><td>" + player.Blocks + "</td><td></td></tr>" +
            "<tr><td>Interceptions</td><td>" + player.Interceptions + "</td><td></td></tr>" +
            "<tr><td>Duels</td><td>" + player.Duels + "</td><td></td></tr>" +
            "<tr><td>Duels Won</td><td>" + player.Duels_won + "</td><td></td></tr>" +
            "<tr><td>Dribbles</td><td>" + player.Dribbles + "</td><td></td></tr>" +
            "<tr><td>Dribbles Pon</td><td>" + player.Dribbles_won + "</td><td></td></tr>" +
            "<tr><td>Dribbles Past</td><td>" + player.Dribbles_past + "</td><td></td></tr>" +
            "<tr><td>Fouls Drawn</td><td>" + player.Fouls_drawn + "</td><td></td></tr>" +
            "<tr><td>Tackles</td><td>" + player.Tackles + "</td><td></td></tr>" +
            "<tr><td>Blocks</td><td>" + player.Blocks + "</td><td></td></tr>" +
            "<tr><td>Interceptions</td><td>" + player.Interceptions + "</td><td></td></tr>" +
            "<tr><td>Duels</td><td>" + player.Duels + "</td><td></td></tr>" +
            "<tr><td>Duels Won</td><td>" + player.Duels_won + "</td><td></td></tr>" +
            "<tr><td>Dribbles</td><td>" + player.Dribbles + "</td><td></td></tr>" +
            "<tr><td>Dribbles Won</td><td>" + player.Dribbles_won + "</td><td></td></tr>" +
            "<tr><td>Dribbles Past</td><td>" + player.Dribbles_past + "</td><td></td></tr>" +
            "<tr><td>Fouls Drawn</td><td>" + player.Fouls_drawn + "</td><td></td></tr>" +
            "<tr><td>Fouls Committed</td><td>" + player.Fouls_committed + "</td><td></td></tr>" +
            "<tr><td>Yellow</td><td>" + player.Yellow + "</td><td></td></tr>" +
            "<tr><td>Red</td><td>" + player.Red + "</td><td></td></tr>" +
            "<tr><td>Penalties Won</td><td>" + player.Penalties_won + "</td><td></td></tr>" +
            "<tr><td>Penalties Conceded</td><td>" + player.Penalties_conceded + "</td><td></td></tr>" +
            "<tr><td>Penalties Scored</td><td>" + player.Penalties_scored + "</td><td></td></tr>" +
            "<tr><td>Penalties Missed</td><td>" + player.Penalties_missed + "</td><td></td></tr>" +
            "<tr><td>Penalties Saved</td><td>" + player.Penalties_saved + "</td><td></td></tr>" +
            "</tbody>" +

            "</table >"
        "</div > ";
    }
    else {
        html =
            "<div class='text-center'>" +
            "<p id=\"tooltip-player-name\" style=\"position: relative; width: 315px; height: 90px; background-color: var(--gray); border: solid black; \"><img class=\"top-performer-image\" width = \"75\" height = \"75\" src =" + playerData.Headshot + " style=\" transform: translate(0px, 5px); \"> " + player.First + " " + player.Last + "</p > " +
            "<table id=\"tooltip-table\" class='table table-scroll table_style' style=\"width: 313px;table-layout: fixed; \">" +
            "<thead><tr><th>Stat</th><th>Actual</th></tr></thead>" +
            "<tbody style=\"max-height: 320px; overflow-y: scroll ;display: inline-block;width: 307px; \">" +
            "<tr><td>Rating</td><td style=\"text-align: end;\">" + player.Rating.toFixed(1) + "</td></tr>" +
            "<tr><td>Goals</td><td style=\"text-align: end;\">" + player.Goals + "</td></tr>" +
            "<tr><td>Assists</td><td style=\"text-align: end;\">" + player.Assists + "</td></tr>" +
            "<tr><td>Passes</td><td style=\"text-align: end;\">" + player.Passes + "</td><td></td></tr>" +
            "<tr><td>Minutes</td><td style=\"text-align: end;\">" + player.Minutes + "</td><td></td></tr>" +
            "<tr><td>Shots</td><td style=\"text-align: end;\">" + player.Shots + "</td></tr>" +
            "<tr><td>Shots on Goal</td><td style=\"text-align: end;\">" + player.Shots_on_goal + "</td><td></td></tr>" +
            "<tr><td>Saves</td><td style=\"text-align: end;\">" + player.Saves + "</td></tr>" +
            "<tr><td>Passes</td><td style=\"text-align: end;\">" + player.Passes + "</td><td></td></tr>" +
            "<tr><td>Key Passes</td><td style=\"text-align: end;\">" + player.Key_passes + "</td>></tr>" +
            "<tr><td>Accurate Passes</td><td style=\"text-align: end;\">" + player.Passes_accurate + "</td><td></td></tr>" +
            "<tr><td>Pass %</td><td style=\"text-align: end;\">" + (player.Pass_pct.toFixed(1) * 100) + "%</td><td></td></tr>" +
            "<tr><td>Tackles</td><td style=\"text-align: end;\">" + player.Tackles + "</td><td></td></tr>" +
            "<tr><td>Blocks</td><td style=\"text-align: end;\">" + player.Blocks + "</td><td></td></tr>" +
            "<tr><td>Interceptions</td><td style=\"text-align: end;\">" + player.Interceptions + "</td><td></td></tr>" +
            "<tr><td>Duels</td><td style=\"text-align: end;\">" + player.Duels + "</td><td></td></tr>" +
            "<tr><td>Duels Won</td><td style=\"text-align: end;\">" + player.Duels_won + "</td><td></td></tr>" +
            "<tr><td>Dribbles</td><td style=\"text-align: end;\">" + player.Dribbles + "</td><td></td></tr>" +
            "<tr><td>Dribbles Pon</td><td style=\"text-align: end;\">" + player.Dribbles_won + "</td><td></td></tr>" +
            "<tr><td>Dribbles Past</td><td style=\"text-align: end;\">" + player.Dribbles_past + "</td><td></td></tr>" +
            "<tr><td>Fouls Drawn</td><td style=\"text-align: end;\">" + player.Fouls_drawn + "</td><td></td></tr>" +
            "<tr><td>Tackles</td><td style=\"text-align: end;\">" + player.Tackles + "</td><td></td></tr>" +
            "<tr><td>Blocks</td><td style=\"text-align: end;\">" + player.Blocks + "</td><td></td></tr>" +
            "<tr><td>Interceptions</td><td style=\"text-align: end;\">" + player.Interceptions + "</td><td></td></tr>" +
            "<tr><td>Duels</td><td style=\"text-align: end;\">" + player.Duels + "</td><td></td></tr>" +
            "<tr><td>Duels Won</td><td style=\"text-align: end;\">" + player.Duels_won + "</td><td></td></tr>" +
            "<tr><td>Dribbles</td><td style=\"text-align: end;\">" + player.Dribbles + "</td><td></td></tr>" +
            "<tr><td>Dribbles Won</td><td style=\"text-align: end;\">" + player.Dribbles_won + "</td><td></td></tr>" +
            "<tr><td>Dribbles Past</td><td style=\"text-align: end;\">" + player.Dribbles_past + "</td><td></td></tr>" +
            "<tr><td>Fouls Drawn</td><td style=\"text-align: end;\">" + player.Fouls_drawn + "</td><td></td></tr>" +
            "<tr><td>Fouls Committed</td><td style=\"text-align: end;\">" + player.Fouls_committed + "</td><td></td></tr>" +
            "<tr><td>Yellow</td><td style=\"text-align: end;\">" + player.Yellow + "</td><td></td></tr>" +
            "<tr><td>Red</td><td style=\"text-align: end;\">" + player.Red + "</td><td></td></tr>" +
            "<tr><td>Penalties Won</td><td style=\"text-align: end;\">" + player.Penalties_won + "</td><td></td></tr>" +
            "<tr><td>Penalties Conceded</td><td style=\"text-align: end;\">" + player.Penalties_conceded + "</td><td></td></tr>" +
            "<tr><td>Penalties Scored</td><td style=\"text-align: end;\">" + player.Penalties_scored + "</td><td></td></tr>" +
            "<tr><td>Penalties Missed</td><td style=\"text-align: end;\">" + player.Penalties_missed + "</td><td></td></tr>" +
            "<tr><td>Penalties Saved</td><td style=\"text-align: end;\">" + player.Penalties_saved + "</td><td></td></tr>" +
            "</tbody>" +

            "</table >"
        "</div > ";
    }
    //if (player.Grid == "1:1") {
    //    html = "<div class='text-center'>" +
    //        "<table class='table table_style'>" +
    //        "<thead><tr><th>Stat</th><th>Actual</th><th>Expected</th></tr></thead>" +
    //        "<tbody style='height:fit-content'>" +
    //        "<tr><td>Goals</td><td>" + player.Goals + "</td><td>(" + xplayer.xGoals.toFixed(1) +")</td></tr>" +
    //        "<tr><td>Assists</td><td>" + player.Assists + "</td><td>(" + xplayer.xAssists.toFixed(1) +")</td></tr>" +
    //        "<tr><td>Passes</td><td>" + player.Passes + "</td></tr>" +
    //        "<tr><td>Shots</td><td>" + player.Shots + "</td><td>(" + xplayer.xShots.toFixed(1) +")</td></tr>" +
    //        "<tr><td>Saves</td><td>" + player.Saves + "</td><td>(" + xplayer.xSaves.toFixed(1) +")</td></tr>" +
    //        "</tbody>" +
    //        "<tfooter>" +
    //        "<tr><td>Player:</td>" + "<td colspan=\"2\">" + player.First + " " + player.Last + "</td></tr>" +
    //        "</tfooter>"

    //    "</table >"
    //    "</div > ";
    //}

    tooltip.innerHTML = html

    let pPlayerName = d3.select("#tooltip-player-name");
    let table = d3.select("#tooltip-table");

    let xpos = x + "px"
    let ypos = y + "px"

    let x2 = x;
    let y2 = y;
    let x2pos = x2 + "px"
    let y2pos = y2 + "px"
    table.style("margin-left", xpos).style("margin-top", ypos);
    pPlayerName.style("left", x2pos).style("top", y2pos);
}

function hoverOff() {
    let tooltips = d3.select("#tooltip")

    tooltips.style("visibility", "hidden");

    d3.select("#outside-div").style("filter", "blur(0px)");
}

function drawPlayerStatsSorting() {
    d3.select("#sort-by-buttons").selectAll("button").on("click", function () {
        d3.select("#sort-by-buttons").selectAll("button").classed("active", false);

        let button = d3.select(this);
        button.classed("active", true);

        var stat = button["_groups"][0][0]["innerHTML"];

        var topPerformers = data.sortedTopRatings;
        var xTopPerformers = data.sortedxTopRatings;

        switch (stat) {
            case "Rating":
                topPerformers = data.sortedTopRatings;
                xTopPerformers = data.sortedxTopRatings;
                break;
            case "Goals":
                topPerformers = data.sortedTopGoals;
                xTopPerformers = data.sortedxTopGoals;
                break;
            case "Assists":
                topPerformers = data.sortedTopAssists;
                xTopPerformers = data.sortedxTopAssists;
                break;
            case "Goals+Assists":
                topPerformers = data.sortedTopGoalsAssists;
                xTopPerformers = data.sortedxTopGoalsAssists;
                break;
            case "Saves":
                topPerformers = data.sortedTopSaves;
                xTopPerformers = data.sortedxTopSaves;
                break;
            case "Passes":
                topPerformers = data.sortedTopPasses;
                xTopPerformers = data.sortedxTopPasses;
                break;
            case "Key Passes":
                topPerformers = data.sortedTopKeyPasses;
                xTopPerformers = data.sortedxTopPasses;
                break;
            case "Pass %":
                topPerformers = data.sortedTopPassPct;
                xTopPerformers = data.sortedxTopPasses;
                break;
            case "Tackles":
                topPerformers = data.sortedTopTackles;
                xTopPerformers = data.sortedxTopTackles;
                break;
            case "Interceptions":
                topPerformers = data.sortedTopInterceptions;
                xTopPerformers = data.sortedxTopInterceptions;
                break;
            case "Dribbles":
                topPerformers = data.sortedTopDribbles;
                xTopPerformers = data.sortedxTopDribbles;
                break;
            case "Fouls":
                topPerformers = data.sortedTopFouls;
                xTopPerformers = data.sortedxTopFouls;
                break;
        }

        let topPerformersList = d3.select("#top-performers");
        let topXPerformersList = d3.select("#top-xperformers");

        topPerformersList.selectAll("li").remove();
        topXPerformersList.selectAll("li").remove();

        let topPerformer1 = topPerformers[0];
        let topPerformer2 = topPerformers[1];
        let topPerformer3 = topPerformers[2];

        let topPerformer1Name = topPerformer1.First + topPerformer1.Last;
        let topPerformer2Name = topPerformer2.First + topPerformer2.Last;
        let topPerformer3Name = topPerformer3.First + topPerformer3.Last;

        let topXPerformer1 = null;
        let topXPerformer2 = null;
        let topXPerformer3 = null;

        let topXPerformer1Name = "";
        let topXPerformer2Name = "";
        let topXPerformer3Name = "";

        if (xTopPerformers.length > 0) {
            topXPerformer1 = xTopPerformers[0];
            topXPerformer2 = xTopPerformers[1];
            topXPerformer3 = xTopPerformers[2];

            topXPerformer1Name = topXPerformer1.First + topXPerformer1.Last;
            topXPerformer2Name = topXPerformer2.First + topXPerformer2.Last;
            topXPerformer3Name = topXPerformer3.First + topXPerformer3.Last;
        }

        if (topPerformer1.Nickname != "") {
            topPerformer1Name = topPerformer1.Nickname;
        }
        if (topPerformer2.Nickname != "") {
            topPerformer2Name = topPerformer2.Nickname;
        }
        if (topPerformer3.Nickname != "") {
            topPerformer3Name = topPerformer3.Nickname;
        }

        if (xTopPerformers.length > 0) {
            if (topXPerformer1.Nickname != "") {
                topXPerformer1Name = topXPerformer1.Nickname;
            }
            if (topXPerformer2.Nickname != "") {
                topXPerformer2Name = topXPerformer2.Nickname;
            }
            if (topXPerformer3.Nickname != "") {
                topXPerformer3Name = topXPerformer3.Nickname;
            }
        }
        var formatter = new Intl.NumberFormat("en-GB", { style: "decimal", signDisplay: "always" });
        // Display top player 1
        if (data.homePlayers.some((x) => x.PID == topPerformer1.PID)) {
            switch (stat) {
                case "Passes":
                case "Key Passes":
                case "Pass %":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer1.PID).Headshot + "\" /></a>" +
                        "<a style=\"font-size:20px; font-weight:bold;\">" + topPerformer1.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer1.PID + "><p>" + topPerformer1Name + "<p>\n<\a>" +
                        "<a> Passes: " + topPerformer1.Passes.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Passes_perf.toFixed(1)) + ") |</a>" +
                        "<a> Key Passes: " + topPerformer1.Key_passes.toFixed(1) + " |</a>" +
                        "<a> Pass %: " + (topPerformer1.Pass_pct * 100).toFixed(1) + "</a>");
                    break
                case "Shots":
                case "Dribbles":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer1.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer1.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer1.PID + "><p>" + topPerformer1Name + "<p>\n<\a>" +
                        "<a> Shots: " + topPerformer1.Shots.toFixed(1) + " |</a>" +
                        "<a> Dri: " + topPerformer1.Dribbles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Dribbles_perf.toFixed(1)) + ")</a>");
                    break
                case "Tackles":
                case "Interceptions":
                case "Fouls":
                case "Yellow":
                case "Red":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer1.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer1.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer1.PID + "><p>" + topPerformer1Name + "<p>\n<\a>" +
                        "<a> Tack.: " + topPerformer1.Shots.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Shots_perf.toFixed(1)) + ") |</a>" +
                        "<a> Int: " + topPerformer1.Interceptions.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Interceptions_perf.toFixed(1)) + ") |</a>" +
                        "<a> Yel: " + topPerformer1.Yellow.toFixed(1) + " |</a>" +
                        "<a> Red: " + topPerformer1.Red.toFixed(1) + "</a>");
                    break
                default:
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer1.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer1.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer1.PID + "><p>" + topPerformer1Name + "<p>\n<\a>" +
                        "<a> G: " + topPerformer1.Goals.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Goals_perf.toFixed(1)) + ") |</a>" +
                        "<a> A: " + topPerformer1.Assists.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Assists_perf.toFixed(1)) + ") |</a>" +
                        "<a> S: " + topPerformer1.Saves.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Saves_perf.toFixed(1)) + ")</a>");
            }
        }
        else {
            switch (stat) {
                case "Passes":
                case "Key Passes":
                case "Pass %":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer1.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer1.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer1.PID + "><p>" + topPerformer1Name + "<p>\n<\a>" +
                        "<a> Passes: " + topPerformer1.Passes.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Passes_perf.toFixed(1)) + ") |</a>" +
                        "<a> Key Passes: " + topPerformer1.Key_passes.toFixed(1) + " |</a>" +
                        "<a> Pass %: " + (topPerformer1.Pass_pct * 100).toFixed(1) + "</a>");
                    break
                case "Shots":
                case "Dribbles":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer1.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer1.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer1.PID + "><p>" + topPerformer1Name + "<p>\n<\a>" +
                        "<a> Shots: " + topPerformer1.Shots.toFixed(1) + " |</a>" +
                        "<a> Dri: " + topPerformer1.Dribbles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Dribbles_perf.toFixed(1)) + ")</a>");
                    break
                case "Tackles":
                case "Interceptions":
                case "Fouls":
                case "Yellow":
                case "Red":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer1.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer1.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer1.PID + "><p>" + topPerformer1Name + "<p>\n<\a>" +
                        "<a> Tack.: " + topPerformer1.Shots.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Shots_perf.toFixed(1)) + ") |</a>" +
                        "<a> Int: " + topPerformer1.Interceptions.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Interceptions_perf.toFixed(1)) + ") |</a>" +
                        "<a> Yel: " + topPerformer1.Yellow.toFixed(1) + " |</a>" +
                        "<a> Red: " + topPerformer1.Red.toFixed(1) + "</a>");
                    break
                default:
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer1.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer1.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer1.PID + "><p>" + topPerformer1Name + "<p>\n<\a>" +
                        "<a> G: " + topPerformer1.Goals.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Goals_perf.toFixed(1)) + ") |</a>" +
                        "<a> A: " + topPerformer1.Assists.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Assists_perf.toFixed(1)) + ") |</a>" +
                        "<a> S: " + topPerformer1.Saves.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer1.PID).Saves_perf.toFixed(1)) + ")</a>");
            }
        }

        // Display top player 2
        if (data.homePlayers.some((x) => x.PID == topPerformer2.PID)) {
            switch (stat) {
                case "Passes":
                case "Key Passes":
                case "Pass %":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer2.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer2.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer2.PID + "><p>" + topPerformer2Name + "<p>\n<\a>" +
                        "<a> Passes: " + topPerformer2.Passes.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Passes_perf.toFixed(1)) + ") |</a>" +
                        "<a> Key Passes: " + topPerformer2.Key_passes.toFixed(1) + " |</a>" +
                        "<a> Pass %: " + (topPerformer2.Pass_pct * 100).toFixed(1) + "</a>");
                    break
                case "Shots":
                case "Dribbles":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer2.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer2.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer2.PID + "><p>" + topPerformer2Name + "<p>\n<\a>" +
                        "<a> Shots: " + topPerformer2.Shots.toFixed(1) + " |</a>" +
                        "<a> Dri: " + topPerformer2.Dribbles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Dribbles_perf.toFixed(1)) + ")</a>");
                    break
                case "Tackles":
                case "Interceptions":
                case "Fouls":
                case "Yellow":
                case "Red":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer2.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer2.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer2.PID + "><p>" + topPerformer2Name + "<p>\n<\a>" +
                        "<a> Tack: " + topPerformer2.Tackles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Shots_perf.toFixed(1)) + ") |</a>" +
                        "<a> Int: " + topPerformer2.Interceptions.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Interceptions_perf.toFixed(1)) + ") |</a>" +
                        "<a> Yel: " + topPerformer2.Yellow.toFixed(1) + " |</a>" +
                        "<a> Red: " + topPerformer2.Red.toFixed(1) + "</a>");
                    break
                default:
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer2.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer2.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer2.PID + "><p>" + topPerformer2Name + "<p>\n<\a>" +
                        "<a> G: " + topPerformer2.Goals.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Goals_perf.toFixed(1)) + ") |</a>" +
                        "<a> A: " + topPerformer2.Assists.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Assists_perf.toFixed(1)) + ") |</a>" +
                        "<a> S: " + topPerformer2.Saves.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Saves_perf.toFixed(1)) + ")</a>");
            }
        }
        else {
            switch (stat) {
                case "Passes":
                case "Key Passes":
                case "Pass %":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer2.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer2.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer2.PID + "><p>" + topPerformer2Name + "<p>\n<\a>" +
                        "<a> Passes: " + topPerformer2.Passes.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Passes_perf.toFixed(1)) + ") |</a>" +
                        "<a> Key Passes: " + topPerformer2.Key_passes.toFixed(1) + " |</a>" +
                        "<a> Pass %: " + (topPerformer2.Pass_pct * 100).toFixed(1) + "</a>");
                    break
                case "Shots":
                case "Dribbles":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer2.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer2.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer2.PID + "><p>" + topPerformer2Name + "<p>\n<\a>" +
                        "<a> Shots: " + topPerformer2.Shots.toFixed(1) + " |</a>" +
                        "<a> Dri: " + topPerformer2.Dribbles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Dribbles_perf.toFixed(1)) + ")</a>");
                    break
                case "Tackles":
                case "Interceptions":
                case "Fouls":
                case "Yellow":
                case "Red":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer2.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer2.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer2.PID + "><p>" + topPerformer2Name + "<p>\n<\a>" +
                        "<a> Tack: " + topPerformer2.Tackles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Shots_perf.toFixed(1)) + ") |</a>" +
                        "<a> Int: " + topPerformer2.Interceptions.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Interceptions_perf.toFixed(1)) + ") |</a>" +
                        "<a> Yel: " + topPerformer2.Yellow.toFixed(1) + " |</a>" +
                        "<a> Red: " + topPerformer2.Red.toFixed(1) + "</a>");
                    break
                default:
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer2.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer2.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer2.PID + "><p>" + topPerformer2Name + "<p>\n<\a>" +
                        "<a> G: " + topPerformer2.Goals.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Goals_perf.toFixed(1)) + ") |</a>" +
                        "<a> A: " + topPerformer2.Assists.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Assists_perf.toFixed(1)) + ") |</a>" +
                        "<a> S: " + topPerformer2.Saves.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer2.PID).Saves_perf.toFixed(1)) + ")</a>");
            }
        }

        // Display top player 3
        if (data.homePlayers.some((x) => x.PID == topPerformer3.PID)) {
            switch (stat) {
                case "Passes":
                case "Key Passes":
                case "Pass %":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer3.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer3.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer3.PID + "><p>" + topPerformer3Name + "<p>\n<\a>" +
                        "<a> Passes: " + topPerformer3.Passes.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Passes_perf.toFixed(1)) + ") |</a>" +
                        "<a> Key Passes: " + topPerformer3.Key_passes.toFixed(1) + " |</a>" +
                        "<a> Pass %: " + (topPerformer3.Pass_pct * 100).toFixed(1) + "</a>");
                    break
                case "Shots":
                case "Dribbles":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer3.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer3.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer3.PID + "><p>" + topPerformer3Name + "<p>\n<\a>" +
                        "<a> Shots: " + topPerformer3.Shots.toFixed(1) + " |</a>" +
                        "<a> Dri: " + topPerformer3.Dribbles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Dribbles_perf.toFixed(1)) + ")</a>");
                    break
                case "Tackles":
                case "Interceptions":
                case "Fouls":
                case "Yellow":
                case "Red":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer3.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer3.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer3.PID + "><p>" + topPerformer3Name + "<p>\n<\a>" +
                        "<a> Tack: " + topPerformer3.Tackles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Shots_perf.toFixed(1)) + ") |</a>" +
                        "<a> Int: " + topPerformer3.Interceptions.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Interceptions_perf.toFixed(1)) + ") |</a>" +
                        "<a> Yel: " + topPerformer3.Yellow.toFixed(1) + " |</a>" +
                        "<a> Red: " + topPerformer3.Red.toFixed(1) + "</a>");
                    break
                default:
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.homePlayers.find((x) => x.PID == topPerformer3.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer3.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer3.PID + "><p>" + topPerformer3Name + "<p>\n<\a>" +
                        "<a> G: " + topPerformer3.Goals.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Goals_perf.toFixed(1)) + ") |</a>" +
                        "<a> A: " + topPerformer3.Assists.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Assists_perf.toFixed(1)) + ") |</a>" +
                        "<a> S: " + topPerformer3.Saves.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Saves_perf.toFixed(1)) + ")</a>");
            }
        }
        else {
            switch (stat) {
                case "Passes":
                case "Key Passes":
                case "Pass %":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer3.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer3.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer3.PID + "><p>" + topPerformer3Name + "<p>\n<\a>" +
                        "<a> Passes: " + topPerformer3.Passes.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Passes_perf.toFixed(1)) + ") |</a>" +
                        "<a> Key Passes: " + topPerformer3.Key_passes.toFixed(1) + " |</a>" +
                        "<a> Pass %: " + (topPerformer3.Pass_pct * 100).toFixed(1) + "</a>");
                    break
                case "Shots":
                case "Dribbles":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer3.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer3.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer3.PID + "><p>" + topPerformer3Name + "<p>\n<\a>" +
                        "<a> Shots: " + topPerformer3.Shots.toFixed(1) + " |</a>" +
                        "<a> Dri: " + topPerformer3.Dribbles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Dribbles_perf.toFixed(1)) + ")</a>");
                    break
                case "Tackles":
                case "Interceptions":
                case "Fouls":
                case "Yellow":
                case "Red":
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer3.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer3.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer3.PID + "><p>" + topPerformer3Name + "<p>\n<\a>" +
                        "<a> Tack: " + topPerformer3.Tackles.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Shots_perf.toFixed(1)) + ") |</a>" +
                        "<a> Int: " + topPerformer3.Interceptions.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Interceptions_perf.toFixed(1)) + ") |</a>" +
                        "<a> Yel: " + topPerformer3.Yellow.toFixed(1) + " |</a>" +
                        "<a> Red: " + topPerformer3.Red.toFixed(1) + "</a>");
                    break
                default:
                    topPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                        + data.awayPlayers.find((x) => x.PID == topPerformer3.PID).Headshot + "\" /></a>" +
                        "<a style=\" font-size:20px; font-weight:bold; \">" + topPerformer3.Rating.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Rating_perf.toFixed(1)) + ")</a>" +
                        "<a href=/Player/Details?id=" + topPerformer3.PID + "><p>" + topPerformer3Name + "<p>\n<\a>" +
                        "<a> G: " + topPerformer3.Goals.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Goals_perf.toFixed(1)) + ") |</a>" +
                        "<a> A: " + topPerformer3.Assists.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Assists_perf.toFixed(1)) + ") |</a>" +
                        "<a> S: " + topPerformer3.Saves.toFixed(1) + " (" + formatter.format(data.allxPlayerStatistics.find((x) => x.PID == topPerformer3.PID).Saves_perf.toFixed(1)) + ")</a>");
            }
        }

        if (xTopPerformers.length > 0) {
            // Display top xplayer 1
            if (data.homePlayers.some((x) => x.PID == topXPerformer1.PID)) {
                switch (stat) {
                    case "Passes":
                    case "Key Passes":
                    case "Pass %":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer1.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer1.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer1.PID + "><p>" + topXPerformer1Name + "<p>\n<\a>" +
                            "<a> xPasses: " + topXPerformer1.xPasses.toFixed(1) + "</a>");
                        break
                    case "Shots":
                    case "Dribbles":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer1.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer1.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer1.PID + "><p>" + topXPerformer1Name + "<p>\n<\a>" +
                            "<a> xShots: " + topXPerformer1.xShots.toFixed(1) + " | </a>" +
                            "<a> xDri: " + topXPerformer1.xDribbles.toFixed(1) + "</a>");
                        break
                    case "Tackles":
                    case "Interceptions":
                    case "Fouls":
                    case "Yellow":
                    case "Red":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer1.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer1.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer1.PID + "><p>" + topXPerformer1Name + "<p>\n<\a>" +
                            "<a> xTack: " + topXPerformer1.xTackles.toFixed(1) + " | </a>" +
                            "<a> xInt: " + topXPerformer1.xInterceptions.toFixed(1) + " | </a>" +
                            "<a> xFoul: " + topXPerformer1.xFouls.toFixed(1) + " | </a>" +
                            "<a> xYel: " + topXPerformer1.xYellow.toFixed(1) + " | </a>" +
                            "<a> xRed: " + topXPerformer1.xRed.toFixed(1) + "</a>");
                        break
                    default:
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer1.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer1.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer1.PID + "><p>" + topXPerformer1Name + "<p>\n<\a>" +
                            "<a> xG: " + topXPerformer1.xGoals.toFixed(1) + " | </a>" +
                            "<a> xA: " + topXPerformer1.xAssists.toFixed(1) + " | </a>" +
                            "<a> xS: " + topXPerformer1.xSaves.toFixed(1) + "</a>");
                }
            }
            else {
                switch (stat) {
                    case "Passes":
                    case "Key Passes":
                    case "Pass %":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer1.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer1.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer1.PID + "><p>" + topXPerformer1Name + "<p>\n<\a>" +
                            "<a> xPasses: " + topXPerformer1.xPasses.toFixed(1) + "</a>");
                        break
                    case "Shots":
                    case "Dribbles":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer1.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer1.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer1.PID + "><p>" + topXPerformer1Name + "<p>\n<\a>" +
                            "<a> xShots: " + topXPerformer1.xShots.toFixed(1) + " | </a>" +
                            "<a> xDri: " + topXPerformer1.xDribbles.toFixed(1) + "</a>");
                        break
                    case "Tackles":
                    case "Interceptions":
                    case "Fouls":
                    case "Yellow":
                    case "Red":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer1.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer1.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer1.PID + "><p>" + topXPerformer1Name + "<p>\n<\a>" +
                            "<a> xTack: " + topXPerformer1.xTackles.toFixed(1) + " | </a>" +
                            "<a> xInt: " + topXPerformer1.xInterceptions.toFixed(1) + " | </a>" +
                            "<a> xFoul: " + topXPerformer1.xFouls.toFixed(1) + " | </a>" +
                            "<a> xYel: " + topXPerformer1.xYellow.toFixed(1) + " | </a>" +
                            "<a> xRed: " + topXPerformer1.xRed.toFixed(1) + "</a>");
                        break
                    default:
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer1.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer1.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer1.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer1.PID + "><p>" + topXPerformer1Name + "<p>\n<\a>" +
                            "<a> xG: " + topXPerformer1.xGoals.toFixed(1) + " | </a>" +
                            "<a> xA: " + topXPerformer1.xAssists.toFixed(1) + " | </a>" +
                            "<a> xS: " + topXPerformer1.xSaves.toFixed(1) + "</a>");
                }
            }

            // Display top player 2
            if (data.homePlayers.some((x) => x.PID == topXPerformer2.PID)) {
                switch (stat) {
                    case "Passes":
                    case "Key Passes":
                    case "Pass %":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer2.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer2.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer2.PID + "><p>" + topXPerformer2Name + "<p>\n<\a>" +
                            "<a> xPasses: " + topXPerformer2.xPasses.toFixed(1) + "</a>");
                        break
                    case "Shots":
                    case "Dribbles":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer2.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer2.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer2.PID + "><p>" + topXPerformer2Name + "<p>\n<\a>" +
                            "<a> xShots: " + topXPerformer2.xShots.toFixed(1) + " | </a>" +
                            "<a> xDri: " + topXPerformer2.xDribbles.toFixed(1) + "</a>");
                        break
                    case "Tackles":
                    case "Interceptions":
                    case "Fouls":
                    case "Yellow":
                    case "Red":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer2.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer2.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer2.PID + "><p>" + topXPerformer2Name + "<p>\n<\a>" +
                            "<a> xTack: " + topXPerformer2.xTackles.toFixed(1) + " | </a>" +
                            "<a> xInt: " + topXPerformer2.xInterceptions.toFixed(1) + " | </a>" +
                            "<a> xFoul: " + topXPerformer2.xFouls.toFixed(1) + " | </a>" +
                            "<a> xYel: " + topXPerformer2.xYellow.toFixed(1) + " | </a>" +
                            "<a> xRed: " + topXPerformer2.xRed.toFixed(1) + "</a>");
                        break
                    default:
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer2.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer2.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer2.PID + "><p>" + topXPerformer2Name + "<p>\n<\a>" +
                            "<a> xG: " + topXPerformer2.xGoals.toFixed(1) + " | </a>" +
                            "<a> xA: " + topXPerformer2.xAssists.toFixed(1) + " | </a>" +
                            "<a> xS: " + topXPerformer2.xSaves.toFixed(1) + "</a>");
                }
            }
            else {
                switch (stat) {
                    case "Passes":
                    case "Key Passes":
                    case "Pass %":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer2.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer2.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer2.PID + "><p>" + topXPerformer2Name + "<p>\n<\a>" +
                            "<a> xPasses: " + topXPerformer2.xPasses.toFixed(1) + "</a>");
                        break
                    case "Shots":
                    case "Dribbles":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer2.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer2.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer2.PID + "><p>" + topXPerformer2Name + "<p>\n<\a>" +
                            "<a> xShots: " + topXPerformer2.xShots.toFixed(1) + " | </a>" +
                            "<a> xDri: " + topXPerformer2.xDribbles.toFixed(1) + "</a>");
                        break
                    case "Tackles":
                    case "Interceptions":
                    case "Fouls":
                    case "Yellow":
                    case "Red":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer2.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer2.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer2.PID + "><p>" + topXPerformer2Name + "<p>\n<\a>" +
                            "<a> xTack: " + topXPerformer2.xTackles.toFixed(1) + " | </a>" +
                            "<a> xInt: " + topXPerformer2.xInterceptions.toFixed(1) + " | </a>" +
                            "<a> xFoul: " + topXPerformer2.xFouls.toFixed(1) + " | </a>" +
                            "<a> xYel: " + topXPerformer2.xYellow.toFixed(1) + " | </a>" +
                            "<a> xRed: " + topXPerformer2.xRed.toFixed(1) + "</a>");
                        break
                    default:
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer2.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer2.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer2.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer2.PID + "><p>" + topXPerformer2Name + "<p>\n<\a>" +
                            "<a> xG: " + topXPerformer2.xGoals.toFixed(1) + " | </a>" +
                            "<a> xA: " + topXPerformer2.xAssists.toFixed(1) + " | </a>" +
                            "<a> xS: " + topXPerformer2.xSaves.toFixed(1) + "</a>");
                }
            }

            // Display top player 3
            if (data.homePlayers.some((x) => x.PID == topXPerformer3.PID)) {
                switch (stat) {
                    case "Passes":
                    case "Key Passes":
                    case "Pass %":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer3.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer3.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer3.PID + "><p>" + topXPerformer3Name + "<p>\n<\a>" +
                            "<a> xPasses: " + topXPerformer3.xPasses.toFixed(1) + "</a>");
                        break
                    case "Shots":
                    case "Dribbles":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer3.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer3.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer3.PID + "><p>" + topXPerformer3Name + "<p>\n<\a>" +
                            "<a> xShots: " + topXPerformer3.xShots.toFixed(1) + " | </a>" +
                            "<a> xDri: " + topXPerformer3.xDribbles.toFixed(1) + "</a>");
                        break
                    case "Tackles":
                    case "Interceptions":
                    case "Fouls":
                    case "Yellow":
                    case "Red":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer3.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer3.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer3.PID + "><p>" + topXPerformer3Name + "<p>\n<\a>" +
                            "<a> xTack: " + topXPerformer3.xTackles.toFixed(1) + " | </a>" +
                            "<a> xInt: " + topXPerformer3.xInterceptions.toFixed(1) + " | </a>" +
                            "<a> xFoul: " + topXPerformer3.xFouls.toFixed(1) + " | </a>" +
                            "<a> xYel: " + topXPerformer3.xYellow.toFixed(1) + " | </a>" +
                            "<a> xRed: " + topXPerformer3.xRed.toFixed(1) + "</a>");
                        break
                    default:
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.homePlayers.find((x) => x.PID == topXPerformer3.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer3.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer3.PID + "><p>" + topXPerformer3Name + "<p>\n<\a>" +
                            "<a> xG: " + topXPerformer3.xGoals.toFixed(1) + " | </a>" +
                            "<a> xA: " + topXPerformer3.xAssists.toFixed(1) + " | </a>" +
                            "<a> xS: " + topXPerformer3.xSaves.toFixed(1) + "</a>");
                }
            }
            else {
                switch (stat) {
                    case "Passes":
                    case "Key Passes":
                    case "Pass %":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer3.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer3.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer3.PID + "><p>" + topXPerformer3Name + "<p>\n<\a>" +
                            "<a> xPasses: " + topXPerformer3.xPasses.toFixed(1) + "</a>");
                        break
                    case "Shots":
                    case "Dribbles":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer3.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer3.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer3.PID + "><p>" + topXPerformer3Name + "<p>\n<\a>" +
                            "<a> xShots: " + topXPerformer3.xShots.toFixed(1) + " | </a>" +
                            "<a> xDri: " + topXPerformer3.xDribbles.toFixed(1) + "</a>");
                        break
                    case "Tackles":
                    case "Interceptions":
                    case "Fouls":
                    case "Yellow":
                    case "Red":
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer3.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer3.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer3.PID + "><p>" + topXPerformer3Name + "<p>\n<\a>" +
                            "<a> xTack: " + topXPerformer3.xTackles.toFixed(1) + " | </a>" +
                            "<a> xInt: " + topXPerformer3.xInterceptions.toFixed(1) + " | </a>" +
                            "<a> xFoul: " + topXPerformer3.xFouls.toFixed(1) + " | </a>" +
                            "<a> xYel: " + topXPerformer3.xYellow.toFixed(1) + " | </a>" +
                            "<a> xRed: " + topXPerformer3.xRed.toFixed(1) + "</a>");
                        break
                    default:
                        topXPerformersList.append('li').attr("class", "player-stat-li").html("<a href=/Player/Details?id=" + topXPerformer3.PID + "><img class=\"top-performer-image\" width=\"75\" height=\"75\" src=\""
                            + data.awayPlayers.find((x) => x.PID == topXPerformer3.PID).Headshot + "\" /></a>" +
                            "<a style=\" font-size:20px; font-weight:bold; \"> x" + topXPerformer3.xRating.toFixed(1) + "</a>" +
                            "<a href=/Player/Details?id=" + topXPerformer3.PID + "><p>" + topXPerformer3Name + "<p>\n<\a>" +
                            "<a> xG: " + topXPerformer3.xGoals.toFixed(1) + " | </a>" +
                            "<a> xA: " + topXPerformer3.xAssists.toFixed(1) + " | </a>" +
                            "<a> xS: " + topXPerformer3.xSaves.toFixed(1) + "</a>");
                }
            }

        }
    });
}
function drawLineups() {
    let player_formations = d3.select("#player-formation-list").selectAll("li");

    let numHome2 = 0;
    let numHome3 = 0;
    let numHome4 = 0;
    let numHome5 = 0;
    let numHome6 = 0;

    let numAway2 = 0;
    let numAway3 = 0;
    let numAway4 = 0;
    let numAway5 = 0;
    let numAway6 = 0;

    let numHomeCols = 0;
    let numAwayCols = 0;

    player_formations["_groups"][0].forEach(function (player) {
        player = d3.select(player);
        if (player.attr("class").length > 7) {
            let player_col = parseInt(player.attr("class").slice(5).split(":")[0].replace('{', '').replace('}', ''));

            if (player.attr("class").includes("home")) {
                switch (player_col) {
                    case 1:
                        break;
                    case 2:
                        numHome2++;
                        break;
                    case 3:
                        numHome3++;
                        break;
                    case 4:
                        numHome4++;
                        break;
                    case 5:
                        numHome5++;
                        break;
                    case 6:
                        numHome6++;
                        break;
                }
                if (player_col > numHomeCols) {
                    numHomeCols = player_col;
                }
            }
            else if (player.attr("class").includes("away")) {
                switch (player_col) {
                    case 1:
                        break;
                    case 2:
                        numAway2++;
                        break;
                    case 3:
                        numAway3++;
                        break;
                    case 4:
                        numAway4++;
                        break;
                    case 5:
                        numAway5++;
                        break;
                    case 6:
                        numAway6++;
                        break;
                }
                if (player_col > numAwayCols) {
                    numAwayCols = player_col;
                }
            }
        }
    });

    player_formations["_groups"][0].forEach(function (player) {
        player = d3.select(player);
        let imgWidth = 680;
        let imgHeight = 450;
        if (player.attr("class").length > 7) {
            let player_col = parseInt(player.attr("class").slice(5).split(":")[0].replace('{', '').replace('}', ''));
            let player_row = parseInt(player.attr("class").slice(5).split(":")[1].replace('{', '').replace('}', ''));

            let imageX = d3.select("#field-image")["_groups"][0][0]["x"];
            let imageY = d3.select("#field-image")["_groups"][0][0]["y"];

            if (player.attr("class").includes("home")) {
                if (player_col == 1) {
                    player_row = 225;
                    player_col = 135;
                }
                else {
                    let homeWidth = imgWidth / 2;
                    let homeHeight = imgHeight;

                    imageX = homeWidth / numHomeCols;

                    if (player_col == 2 && numHome2 > 0) {
                        player_row--;

                        let home2Height = homeHeight / numHome2;
                        player_row = (player_row * home2Height) + (home2Height * .5);

                        let home2Width = homeWidth / numHomeCols;
                        player_col = (imageX) + (player_col * home2Width);
                    }
                    else if (player_col == 3 && numHome3 > 0) {
                        player_row--;

                        let home3Height = homeHeight / numHome3;
                        player_row = (player_row * home3Height) + (home3Height * .5);

                        let home3Width = homeWidth / numHomeCols;
                        player_col = (imageX) + (player_col * home3Width);
                    }
                    else if (player_col == 4 && numHome4 > 0) {
                        player_row--;

                        let home4Height = homeHeight / numHome4;
                        player_row = (player_row * home4Height) + (home4Height * .5);

                        let home4Width = homeWidth / numHomeCols;
                        player_col = (imageX) + (player_col * home4Width);
                    }
                    else if (player_col == 5 && numHome5 > 0) {
                        player_row--;

                        let home5Height = homeHeight / numHome5;
                        player_row = (player_row * home5Height) + (home5Height * .5);

                        let home5Width = homeWidth / numHomeCols;
                        player_col = (imageX) + (player_col * home5Width);
                    }
                    else if (player_col == 6 && numHome6 > 0) {
                        player_row--;

                        let home6Height = homeHeight / numHome6;
                        player_row = (player_row * home6Height) + (home6Height * .5);

                        let home6Width = homeWidth / numHomeCols;
                        player_col = (imageX) + (player_col * home6Width);
                    }
                }
            }

            else if (player.attr("class").includes("away")) {
                if (player_col == 1) {
                    player_row = 225;
                    player_col = 805;
                }
                else {
                    //if (numAwayCols == 4) {
                    //    imageX += 4.4 * imgWidth;
                    //}
                    //else if (numAwayCols == 5) {
                    //    imageX += 5.4 * imgWidth;
                    //}
                    //else if (numAwayCols == 6) {
                    //    imageX += 6.4 * imgWidth;
                    //}

                    let awayWidth = imgWidth / 2;
                    let awayHeight = imgHeight;

                    imageX = (imgWidth + imageX + 140) - (awayWidth / numAwayCols);

                    if (player_col == 2 && numAway2 > 0) {
                        player_row--;

                        let away2Height = awayHeight / numAway2;
                        player_row = (player_row * away2Height) + (away2Height * .5);

                        let away2Width = awayWidth / numAwayCols;
                        player_col = (imageX) - (player_col * away2Width);
                    }
                    else if (player_col == 3 && numAway3 > 0) {
                        player_row--;

                        let away3Height = awayHeight / numAway3;
                        player_row = (player_row * away3Height) + (away3Height * .5);

                        let away3Width = awayWidth / numAwayCols;
                        player_col = (imageX) - (player_col * away3Width);
                    }
                    else if (player_col == 4 && numAway4 > 0) {
                        player_row--;

                        let away4Height = awayHeight / numAway4;
                        player_row = (player_row * away4Height) + (away4Height * .5);

                        let away4Width = awayWidth / numAwayCols;
                        player_col = (imageX) - (player_col * away4Width);
                    }
                    else if (player_col == 5 && numAway5 > 0) {
                        player_row--;

                        let away5Height = awayHeight / numAway5;
                        player_row = (player_row * away5Height) + (away5Height * .5);

                        let away5Width = awayWidth / numAwayCols;
                        player_col = (imageX) - (player_col * away5Width);
                    }
                    else if (player_col == 6 && numAway6 > 0) {
                        player_row--;

                        let away6Height = awayHeight / numAway6;
                        player_row = (player_row * away6Height) + (away6Height * .5);

                        let away6Width = awayWidth / numAwayCols;
                        player_col = (imageX) - (player_col * away6Width);
                    }
                }
            }

            let translate_string = "translate(" + (player_col - 75) + "px," + (player_row + 50) + "px)";
            player.style("transform", translate_string);
        }
    });

    fixImages();
}
function drawTimeline() {
    const CHART_WIDTH = 1500
    const CHART_HEIGHT = 300
    const MARGIN = 50

    var tooltip = d3.select("#match-timeline")
        .append("div")
        .style("position", "absolute")
        .style("visibility", "hidden")
        .style('background-color', 'white')
        .style("border", "solid")
        .style("border-color", "#000000")
        .style("border-width", "3px")
        .style("border-radius", "5px")
        .style('padding', '10px')
        .style('text-align', 'center')


    let eventData = data.eventsInfoRecords

    let homeTeam = data.matchesInfoRecord.HomeName
    let awayTeam = data.matchesInfoRecord.AwayName

    let timeline = d3.select("#match-timeline")
        .append("svg")
        .attr("width", CHART_WIDTH)
        .attr("height", CHART_HEIGHT)
    //.style("border", "solid red")

    let maxMinute = data.matchesInfoRecord.Elapsed;

    let xScale = d3.scaleLinear()
        .range([MARGIN, CHART_WIDTH - MARGIN])
        .domain([0, maxMinute])


    timeline.append("g")
        .attr("transform", "translate(0," + CHART_HEIGHT / 2 + ")")
        .call(d3.axisBottom(xScale))//.tickValues(xScale.ticks(6).concat("FT")).tickSize(10))

    var icons = timeline.append("g")

    var homeEvents = timeline.append("g")
    var homeData = eventData.filter((d) => { return d.TeamName == homeTeam })
    var awayEvents = timeline.append("g")
    var awayData = eventData.filter((d) => { return d.TeamName == awayTeam })

    homeEvents.selectAll("text")
        .data(homeData)
        .enter()
        .append("text")
        .style("fill", (d) => {
            if (d.Detail == "Yellow Card") {
                return "#FFFF00"
            }
            else if (d.Detail == "Red Card") {
                return "#EB5160"
            }
        })
        .attr("class", "fa")
        .attr('font-family', 'Font Awesome')
        .attr("x", (d) => { return xScale(d.Minute) })
        .attr("y", (d, i) => {
            var yPosition = (CHART_HEIGHT / 2) - 15;
            if (d.Minute < 5) {
                yPosition = (CHART_HEIGHT / 2) - 25
                if (i != 0) {
                    if (d.Minute - homeData[i - 1].Minute <= 2) {
                        yPosition -= 30
                    }
                }
            }
            else {
                if (i != 0) {
                    if (d.Minute - homeData[i - 1].Minute <= 2) {
                        yPosition = homeData[i - 1].yPosition - 30
                    }
                }
            }

            d.yPosition = yPosition;
            return yPosition;

        })
        .style("font-size", "25px")
        .text((d) => {
            if (d.Detail == "Normal Goal") {
                return "\uf1e3"
            }
            else if (d.Detail == "Yellow Card" || d.Detail == "Red Card") {
                return "\uf10b"
            }
            else {
                return "\ue068"
            }
        })
        .attr("text-anchor", "middle")
        .on("mouseover", (event, d) => {
            if (event.Type == "subst") {
                var onName = event.Last//event.First + " " + event.Last
                var offName = event.AssistLast;
                var minute = event.Minute
                var event = event.Detail

                return tooltip
                    .style("visibility", "visible")
                    .html('<p><b>On:</b> ' + onName + ' -> <b>Off:</b> ' + offName + '</p><p> Minute: ' + minute + '</p>')
            }
            else {
                var name = event.Last;
                var minute = event.Minute;
                var event = event.Detail;
                return tooltip
                    .style("visibility", "visible")
                    .html("<p>" + name + " - " + event + "</p><p> Minute: " + minute + '</p>')
            }
        })
        .on("mousemove", function (event, d) { return tooltip.style("top", (d3.mouse(d3.event.target)[1]) + -155 + "px").style("left", (d3.mouse(d3.event.target)[0]) + 10 + "px"); })
        .on("mouseout", function () { return tooltip.style("visibility", "hidden"); });

    awayEvents.selectAll("text")
        .data(awayData)
        .enter()
        .append("text")
        .style("fill", (d) => {
            if (d.Detail == "Yellow Card") {
                return "#FFFF00"
            }
            else if (d.Detail == "Red Card") {
                return "#EB5160"
            }
        })
        .attr("class", "fa")
        .attr('font-family', 'Font Awesome')
        .attr("x", (d) => { return xScale(d.Minute) })
        .attr("y", (d, i) => {
            var yPosition = (CHART_HEIGHT / 2) + 30;
            if (d.Minute < 5) {
                yPosition = (CHART_HEIGHT / 2) + 40
                if (i != 0) {
                    if (d.Minute - awayData[i - 1].Minute <= 2) {
                        yPosition += 30
                    }
                }
            }
            else {
                if (i != 0) {
                    if (d.Minute - awayData[i - 1].Minute <= 2) {
                        yPosition = awayData[i - 1].yPosition + 30
                    }
                }
            }

            d.yPosition = yPosition;
            return yPosition;

        })
        .style("font-size", "25px")
        .text((d) => {
            if (d.Detail == "Normal Goal") {
                return "\uf1e3"
            }
            else if (d.Detail == "Yellow Card" || d.Detail == "Red Card") {
                return "\uf10b"
            }
            else {
                return "\ue068"
            }
        })
        .attr("text-anchor", "middle")
        .on("mouseover", (event, d) => {
            if (event.Type == "subst") {
                var onName = event.Last//event.First + " " + event.Last
                var offName = event.AssistLast;
                var minute = event.Minute
                var event = event.Detail

                return tooltip
                    .style("visibility", "visible")
                    .html('<p><b>On:</b> ' + onName + ' -> <b>Off:</b> ' + offName + '</p><p> Minute: ' + minute + '</p>')
            }
            else {
                var name = event.Last;
                var minute = event.Minute;
                var event = event.Detail;

                return tooltip
                    .style("visibility", "visible")
                    .html("<p>" + name + " - " + event + "</p><p> Minute: " + minute + '</p>')
            }
        })
        .on("mousemove", function (event, d) { return tooltip.style("top", (d3.mouse(d3.event.target)[1]) + -155 + "px").style("left", (d3.mouse(d3.event.target)[0]) + 10 + "px"); })
        .on("mouseout", function () { return tooltip.style("visibility", "hidden"); });

    //homeEvents.selectAll("text")
    //.data(eventData.filter((d) => {return d.TeamName == homeTeam})
    //.enter()
    //.append("text")
    //.style("fill", (d) => {
    //    if (d.Detail == "Yellow Card") {
    //        return "#E4DE25"
    //    }
    //    else if (d.Detail == "Red Card") {
    //        return "#EB5160"
    //    }
    //})
    //.attr("class", "fa")
    //.attr('font-family', 'Font Awesome')
    //.attr("x", (d) => { return xScale(d.Minute) })
    //.attr("y", (d) => {
    //    if (d.TeamName == homeTeam) {
    //        return (CHART_HEIGHT / 2) - 15
    //    } else {
    //        return (CHART_HEIGHT / 2) + 30
    //    }
    //})
    //.attr("font-size", "20px")
    //.text((d) => {
    //    if (d.Detail == "Normal Goal") {
    //        return "\uf1e3"
    //    }
    //    else if (d.Detail == "Yellow Card" || d.Detail == "Red Card") {
    //        return "\uf10b"
    //    }
    //    else {
    //        return "\ue068"
    //    }
    //})
    //.attr("text-anchor", "middle")
    //.on("mouseover", (event, d) => {
    //    if (event.Type == "subst") {
    //        var onName = event.Last//event.First + " " + event.Last
    //        var offName = event.AssistLast;
    //        var minute = event.Minute
    //        var event = event.Detail

    //        return tooltip
    //            .style("visibility", "visible")
    //            .html('<p><b>On:</b> ' + onName + ' -> <b>Off:</b> ' + offName + '</p>')
    //    }
    //    else {
    //        var name = event.Last;
    //        var event = event.Detail;
    //        return tooltip
    //            .style("visibility", "visible")
    //            .html("<p>" + name + " - " + event + "</p>")
    //    }
    //})
    //.on("mousemove", function (event, d) { return tooltip.style("top", (d3.mouse(d3.event.target)[1]) + -155 + "px").style("left", (d3.mouse(d3.event.target)[0]) + 10 + "px"); })
    //.on("mouseout", function () { return tooltip.style("visibility", "hidden"); });

    timeline.append("rect")
        .attr("x", MARGIN)
        .attr("y", (CHART_HEIGHT / 2) - 8)
        .attr("width", CHART_WIDTH - (MARGIN * 2) + 15)
        .attr("height", 16)
        .attr("rx", 8)
        .attr("fill", "#4AC428")

    var skip = false
    var skipCnt = 0

    var timeStamps = eventData.map((d) => {
        return d.Minute;
    })

    timeStamps.forEach((minute, i) => {
        for (let idx = i + 1; idx < timeStamps.length; idx++) {
            if (minute == 0) {
                continue;
            }

            if (minute > 2) {
                var diff = timeStamps[idx] - minute;
                if (diff <= 2) {
                    timeStamps[idx] = 0;
                }
                else if (diff > 2) {
                    break;
                }
            }
            else {
                timeStamps[i] = 0;
                break;
            }
        }
    })

    timeline.append("g").selectAll("text")
        .data(timeStamps)
        .enter()
        .append("text")
        .text((d) => { if (d > 0) return d })
        .attr("x", (d) => { if (d > 0) return xScale(d) })
        .attr("y", CHART_HEIGHT / 2 + 6)
        .attr("text-anchor", "middle")
        .style("fill", "white")


    timeline.append("text")
        .text("Home")
        .attr("x", MARGIN + 5)
        .attr("y", (CHART_HEIGHT / 2) - 10)

    timeline.append("text")
        .text("Away")
        .attr("x", MARGIN + 5)
        .attr("y", (CHART_HEIGHT / 2) + 20)

    timeline.append("text")
        .text("KO")
        .attr("x", MARGIN + 5)
        .attr("y", (CHART_HEIGHT / 2) + 6)
        .style("fill", "white")
}
function getHomeStatistics() {
    let homeData = data.homeStatistics;
    let yellows = homeData["Yellows"];
    let reds = homeData["Reds"];
    let cards = reds + yellows;
    return [data.homeScore, homeData["Shots"], homeData["Conceded"], homeData["Fouls"], cards, homeData["Saves"]];
}
function getxHomeStatistics() {
    let xHomeData = data.xHomeStatistics;
    let yellows = xHomeData["xYellow"];
    let reds = xHomeData["xRed"];
    let cards = yellows + reds;
    return [xHomeData["xGoals"], xHomeData["xShots"], 0, xHomeData["xFouls"], cards, xHomeData["xSaves"]];
}
function getAwayStatistics() {
    let awayData = data.awayStatistics;
    let yellows = awayData["Yellows"];
    let reds = awayData["Reds"];
    let cards = reds + yellows;
    return [data.awayScore, awayData["Shots"], awayData["Conceded"], awayData["Fouls"], cards, awayData["Saves"]];
}
function getxAwayStatistics() {
    let xAwayData = data.xAwayStatistics;
    let yellows = xAwayData["xYellow"];
    let reds = xAwayData["xRed"];
    let cards = yellows + reds;
    return [xAwayData["xGoals"], xAwayData["xShots"], 0, xAwayData["xFouls"], cards, xAwayData["xSaves"]];
}
function drawTeamCompare() {
    let homeData = getHomeStatistics();
    let xHomeData = getxHomeStatistics();
    let awayData = getAwayStatistics();
    let xAwayData = getxAwayStatistics();

    let svg = d3.select("#team-comparisons");
    let width = 800;
    let height = 550;
    svg.attr("width", width)
        .attr("height", height)
        .style("border", "solid red")
        .append("g");

    // Add legend
    svg.append("circle").attr("cx", width - 100).attr("cy", 60).attr("r", 6).style("fill", "#4984b8")
    svg.append("circle").attr("cx", width - 100).attr("cy", 90).attr("r", 6).style("fill", "#880808")
    svg.append("circle").attr("cx", width - 100).attr("cy", 120).attr("r", 6).style("fill", "#4984b8").style("fill-opacity", "0.5").style("stroke", "black").style("stroke-width", "2px");
    svg.append("circle").attr("cx", width - 100).attr("cy", 150).attr("r", 6).style("fill", "#880808").style("fill-opacity", "0.5").style("stroke", "black").style("stroke-width", "2px");
    svg.append("text").attr("x", width - 80).attr("y", 60).text("Home").style("font-size", "15px").attr("alignment-baseline", "middle")
    svg.append("text").attr("x", width - 80).attr("y", 90).text("Away").style("font-size", "15px").attr("alignment-baseline", "middle")
    svg.append("text").attr("x", width - 80).attr("y", 120).text("xHome").style("font-size", "15px").attr("alignment-baseline", "middle")
    svg.append("text").attr("x", width - 80).attr("y", 150).text("xAway").style("font-size", "15px").attr("alignment-baseline", "middle")

    let max = d3.max(homeData);
    max = Math.max(max, d3.max(awayData));
    max = Math.max(max, d3.max(xAwayData));
    max = Math.max(max, d3.max(xHomeData));

    var groups = ["Goals", "Shots", "Conceded", "Fouls", "Cards", "Saves"];

    var x = d3.scaleBand()
        .domain(groups)
        .range([0, width])
        .padding([0.2]);

    svg.append("g")
        .style("transform", "translate(0px, 520px)")
        .call(d3.axisBottom(x).tickSize(0));

    var y = d3.scaleLinear()
        .domain([0, max])
        .range([height - 50, 0]);
    y.nice();

    svg.append("g")
        .style("transform", "translate(30px, 20px)")
        .call(d3.axisLeft(y));

    let homeGroupData = [];

    homeGroupData.push({
        key: "Goals",
        value: homeData[0]
    });
    homeGroupData.push({
        key: "Shots",
        value: homeData[1]
    });
    homeGroupData.push({
        key: "Conceded",
        value: homeData[2]
    });
    homeGroupData.push({
        key: "Fouls",
        value: homeData[3]
    });
    homeGroupData.push({
        key: "Cards",
        value: homeData[4]
    });
    homeGroupData.push({
        key: "Saves",
        value: homeData[5]
    });

    let xHomeGroupData = [];

    xHomeGroupData.push({
        key: "Goals",
        value: xHomeData[0]
    });
    xHomeGroupData.push({
        key: "Shots",
        value: xHomeData[1]
    });
    xHomeGroupData.push({
        key: "Conceded",
        value: xHomeData[2]
    });
    xHomeGroupData.push({
        key: "Fouls",
        value: xHomeData[3]
    });
    xHomeGroupData.push({
        key: "Cards",
        value: xHomeData[4]
    });
    xHomeGroupData.push({
        key: "Saves",
        value: xHomeData[5]
    });

    let awayGroupData = [];

    awayGroupData.push({
        key: "Goals",
        value: awayData[0]
    });
    awayGroupData.push({
        key: "Shots",
        value: awayData[1]
    });
    awayGroupData.push({
        key: "Conceded",
        value: awayData[2]
    });
    awayGroupData.push({
        key: "Fouls",
        value: awayData[3]
    });
    awayGroupData.push({
        key: "Cards",
        value: awayData[4]
    });
    awayGroupData.push({
        key: "Saves",
        value: awayData[5]
    });

    let xAwayGroupData = [];

    xAwayGroupData.push({
        key: "Goals",
        value: xAwayData[0]
    });
    xAwayGroupData.push({
        key: "Shots",
        value: xAwayData[1]
    });
    xAwayGroupData.push({
        key: "Conceded",
        value: xAwayData[2]
    });
    xAwayGroupData.push({
        key: "Fouls",
        value: xAwayData[3]
    });
    xAwayGroupData.push({
        key: "Cards",
        value: xAwayData[4]
    });
    xAwayGroupData.push({
        key: "Saves",
        value: xAwayData[5]
    });

    var xSubGroup = d3.scaleBand()
        .domain(groups)
        .range([0, 0]);

    // Home bars
    svg.append("g")
        .attr("id", "home-bars")
        .selectAll("g")
        .data(homeGroupData)
        .enter()
        .append("g")
        .style("transform", function (d) {
            return "translate(" + (x(d.key) + 15) + "px, 20px)";
        })
        .selectAll("rect")
        .data(d => [d])
        .enter().append("rect")
        .attr("x", function (d) { return xSubGroup(d.key); })
        .attr("y", function (d) { return y(d.value); })
        .attr("width", 30)
        .attr("height", function (d) { return height - 50 - y(d.value); })
        .attr("fill", function (d) { return "#4984b8" });

    // Home xStat bars
    svg.append("g")
        .attr("id", "home-bars")
        .selectAll("g")
        .data(xHomeGroupData)
        .enter()
        .append("g")
        .style("transform", function (d) {
            return "translate(" + (x(d.key) + 15) + "px, 20px)";
        })
        .selectAll("rect")
        .data(d => [d])
        .enter().append("rect")
        .attr("x", function (d) { return xSubGroup(d.key); })
        .attr("y", function (d) { return y(d.value); })
        .attr("width", 30)
        .attr("height", function (d) { return height - 50 - y(d.value); })
        .attr("fill", "#4984b8")
        .attr("stroke", "black")
        .attr("stroke-opacity", "0.75")
        .attr("stroke-width", "2px")
        .attr("fill-opacity", "0.2");

    // Away bars
    svg.append("g")
        .attr("id", "away-bars")
        .selectAll("g")
        .data(awayGroupData)
        .enter()
        .append("g")
        .style("transform", function (d) {
            return "translate(" + (x(d.key) + 55) + "px, 20px)";
        })
        .selectAll("rect")
        .data(d => [d])
        .enter().append("rect")
        .attr("x", function (d) { return xSubGroup(d.key); })
        .attr("y", function (d) { return y(d.value); })
        .attr("width", 30)
        .attr("height", function (d) { return height - 50 - y(d.value); })
        .attr("fill", function (d) { return "#880808" });

    // Away xStat bars
    svg.append("g")
        .attr("id", "away-bars")
        .selectAll("g")
        .data(xAwayGroupData)
        .enter()
        .append("g")
        .style("transform", function (d) {
            return "translate(" + (x(d.key) + 55) + "px, 20px)";
        })
        .selectAll("rect")
        .data(d => [d])
        .enter().append("rect")
        .attr("x", function (d) { return xSubGroup(d.key); })
        .attr("y", function (d) { return y(d.value); })
        .attr("width", 30)
        .attr("height", function (d) { return height - 50 - y(d.value); })
        .attr("stroke", "black")
        .attr("stroke-width", "2px")
        .attr("stroke-opacity", "0.75")
        .attr("fill", "#880808")
        .attr("fill-opacity", "0.2");

}
function fixImages() {
    var images = document.images;

    for (var i = 0; i < images.length; i++) {
        //images[i].classList.add("image-backup");
        images[i].onerror = function () {
            if (this.className == "team-badge-image") {
                this.src = "/images/default-team-image.png";
                this.width = 200;
            }
            else if (this.className == "player-lineup-image") {
                this.src = "/images/default-player-image.png";
                this.width = 50;
            }
            else if (this.className == "top-performer-image") {
                this.src = "/images/default-player-image.png";
                this.width = 75;
            }
            else if (this.className == "team-lineups-image") {
                this.src = "/images/default-player-image.png";
                this.width = 90;
            }
            else if (this.className == "team-stats-badge") {
                this.src = "/images/default-player-image.png";
                this.width = 100;
            }
            else if (this.className == "previous-matches-badge") {
                this.src = "/images/default-team-image.png";
                this.width = 50;
            }
        }
    }
}

