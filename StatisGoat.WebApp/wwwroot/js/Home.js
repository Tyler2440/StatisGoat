AddLeagueSelectionListeners()
var currentLeagueSelection = "All Leagues"
var currentDateSelection = ""

jSuites.calendar(document.getElementById('calendar'), {
    onupdate: function (element, new_date) {
        currentDateSelection = new_date
        UpdateMatchSelection(currentLeagueSelection, currentLeagueSelection)   

        var today = new Date();
        var dd = String(today.getDate()).padStart(2, '0');
        var mm = String(today.getMonth() + 1).padStart(2, '0');
        var yyyy = today.getFullYear();

        today = yyyy + '-' + mm + '-' + dd;

        var selected_date = new_date.substring(0, 10)

        if (selected_date == today) {
            document.getElementById("home-header").innerText = 'Today\'s Matches'
        }
        else {
            document.getElementById("home-header").innerText = 'Matches for: ' + selected_date
        }

    }
});

fixImages();

function AddLeagueSelectionListeners() {
    let leaguesTable = d3.select('#leagues-table')
    let leagueTableBody = leaguesTable.selectAll("tbody")

    leagueTableBody.selectAll("tr")
        .on("mouseover", function (d) {
            d3.select(this).style("background", "rgb(187, 187, 187)")
        })
        .on("mouseout", function (d) {
            
            d3.select(this).style("background", "none")
        })
        .on("click", function (event, d) {

            leagueTableBody.selectAll("tr")
                .classed("selected-league", false)

            d3.select(this).classed("selected-league", true)

            currentLeagueSelection = d3.select(this)["_groups"][0][0].innerText
            UpdateMatchSelection()
        })
}


function UpdateMatchSelection() {
    var date = currentDateSelection
    var data = { date: date };
    updateTable();

    let previousTopPlayers = d3.select("#top-players");
    previousTopPlayers.selectAll("tr").remove();

    $("#spinner").show();
    d3.select("#calendar").style("pointer-events", "none");
    $.ajax({
        type: "GET",
        url: "/Home/ChangeDate",
        data: data,
        
        success: function (data) {
            let matches = data.item1;
            let topPlayers = data.item2;
            if (currentLeagueSelection != "All Leagues") {
                matches = matches.filter(function (d) { return d.competition == currentLeagueSelection })
            }
           
            let tableBody = d3.select("#matches-table-body");
            let tr = tableBody
                .selectAll("tr")
                .data(matches)
                .enter()
                .append("tr");

            if (matches.length == 0) {
                let noGamesRow = tableBody.append("tr");
                noGamesRow.append("td");
                noGamesRow.append("td");
                noGamesRow.append("td").html("<a>No games found!</a>")
                    .attr("class", "text-center");
                noGamesRow.append("td");
                noGamesRow.append("td");
            }
            console.log("data");
            console.log(data);
            // let x = d3.scaleLinear().domain(d3.max([d,]))

            tr
                .append("td")
                .html(function (d) {
                    console.log(d.dateTime)
                    let date = new Date(d.dateTime);
                    let offset = new Date().getTimezoneOffset()/60;
                    date.setHours(date.getHours() + offset);
                    return "<a>" + date.toLocaleTimeString("en-US", { hour: '2-digit', minute: '2-digit' }) + " MST</a>";
                })
                .attr("class", "text-center");

            tr
                .append("td")
                .html(d => "<a href=\"/Team/Details?id=" + d.homeId + "\"><img src=\"" + d.homeBadge + "\"><p><a href=\"/Team/Details?id=" + d.homeId + "\">" + d.homeName + "</a></p>")
                .attr('class', 'text-center')

            scoreTd = tr.append("td")
                .html(d => "<p><a>" + d.result + "</a></p>")

            var bopWidth = 300
            var bopHeight = 25
            var bopSVG = scoreTd.append("svg")
                .attr("width", bopWidth)
                .attr("height", bopHeight)
                .style("border", "solid black 2px")

            scaleX = d3.scaleLinear()
                .domain([0,1])
                .range([0, bopWidth])

            //home
            bopSVG.append('rect')
                .attr("x",0)
                .attr("y",0)
                .attr("width", (d) => { return scaleX(d.homepct)})
                .attr("height", 25)
                .attr("fill", "#CB0025")

            bopSVG.append('text')
                .attr("transform", (d) => {
                    let scale = scaleX(d.homepct);
                    if (d.homepct > .10 && d.homepct < .15) {
                        return "translate(" + ((scale / 2.9) - 5) + ",16)";
                    }
                    else {
                        return "translate(" + scale / 2.9 + ",16)"
                    }
                })
                .style("fill", "white")
                .text((d) => { if (d.homepct < .10) { return "" } else { return Math.round(d.homepct * 100) + "%" }});

            //draw
            bopSVG.append('rect')
                .attr("x", (d) => { return scaleX(d.homepct) })
                .attr("y", 0)
                .attr("width", (d) => { return scaleX(d.drawpct) })
                .attr("height", 25)
                .attr("fill", "#8A8A8A")

            bopSVG.append('text')
                .attr("transform", (d) => {
                    let scale = scaleX(d.homepct) + (scaleX(d.drawpct) / 2.9);
                    if (d.drawpct > .10 && d.drawpct < .15) {
                        return "translate(" + (scale - 5) + ",16)";
                    }
                    else {
                        return "translate(" + scale + ",16)"
                    }
                })
                .style("fill", "white")
                .text((d) => { if (d.drawpct < .10) { return "" } else { return Math.round(d.drawpct * 100) + "%" } });

            //away
            bopSVG.append('rect')
                .attr("x", (d) => { return scaleX(d.drawpct) + scaleX(d.homepct) })
                .attr("y", 0)
                .attr("width", (d) => { return scaleX(d.awaypct) })
                .attr("height", 25)
                .attr("fill", "#5C7AFF");

            bopSVG.append('text')
                .attr("transform", (d) => {
                    let scale = scaleX(d.drawpct) + scaleX(d.homepct) + (scaleX(d.awaypct) / 2.9);
                    if (d.awaypct > .10 && d.awaypct < .15) {
                        return "translate(" + (scale - 5) + ",16)";
                    }
                    else {
                        return "translate(" + scale + ",16)"
                    }
                })
                .style("fill", "white")
                .text((d) => { if (d.awaypct < .10) { return "" } else { return Math.round(d.awaypct * 100) + "%" } });

            tr
                .append("td")
                .html(d => "<a href=\"/Team/Details?id=" + d.awayId + "\"><img src=\"" + d.awayBadge + "\"><p><a href=\"/Team/Details?id=" + d.awayId + "\">" + d.awayName + "</a></p>")
                .attr('class', 'text-center')

            tr
                .append("td")
                .html(d => "<a style=\"margin - right: 10px\" href=\"\/Match\/Details?id=" + d.matchId + "\">Details<\/a>")
                .attr('class', 'text-center')

            let playersTr = previousTopPlayers.selectAll("tr")
                .data(topPlayers)
                .enter()
                .append("tr");

            playersTr.append("td")
                .html(function (d) {
                    if (d.nickname != "") {
                        return "<a href=\"/Player/Details?id=" + d.pid + "\"><p>" + d.nickname + " (" + (Math.round(d.rating * 100) / 100) + ")</p></a>";
                    }
                    else {
                        return "<a href=\"/Player/Details?id=" + d.pid + "\"><p>" + d.first + " " + d.last + " (" + (Math.round(d.rating * 100) / 100) + ")</p></a>";
                    }
                })

            let favteamsTable = d3.select("#favorite-teams-body");

            let favteamstr = favteamsTable.selectAll("tr").remove();

            if (favTeams != null && favTeams.length > 0) {
                matches.forEach(function (match) {
                    if (favTeams.includes(match.homeId) || favTeams.includes(match.awayId)) {
                        favteamsTable.append("tr").html(
                            "<td><a href=\"Team/Details?id=" + match.homeId + "\"><img width=\"60\" height=\"60\" src=\"" + match.homeBadge + "\"/></a></td>" +
                            "<td width=\"50px\">" + match.result + "</td>" +
                            "<td><a href=\"Team/Details?id=" + match.awayId + "\"><img width=\"60\" height=\"60\" src=\"" + match.awayBadge + "\"/></a></td>" +
                            "<td><a style=\"margin-right:10px\" href=\"Match/Details?id=" + match.matchId + "\">Details</a></td>");
                    }
                });
            }
        }, 
        complete: function () {
            $("#spinner").hide()
            d3.select("#calendar").style("pointer-events", "all");
        },
        fail: function () {
            $("spinner").hide();
        }
    });
}

function updateTable() {
    var table = d3.select("#matches-table-body");
    table.selectAll("tr").remove();
}

function fixImages() {
    var images = document.images;

    for (var i = 0; i < images.length; i++) {
        //images[i].classList.add("image-backup");
        images[i].onerror = function () {
            this.src = "/images/default-team-image.png";
            this.width = 50;
        }
    }
}