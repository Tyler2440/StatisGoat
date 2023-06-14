fixImages();
DisplayTeamsList();

//setTimeout(100);

window.addEventListener('load', function () {
    fixImages();
})

d3.select("#sort-by-buttons").selectAll("button").on("click", function () {
    d3.select("#sort-by-buttons").selectAll("button").classed("active", false);

    let button = d3.select(this);
    button.classed("active", true);

    var stat = button["_groups"][0][0]["innerHTML"];

    let teams = data;
    let newTeams = [];

    teams.forEach(team => {
        if (stat == "Others") {
            if (team.Nation != "England" && team.Nation != "Spain" && team.Nation != "Germany" && team.Nation != "Italy" && team.Nation != "France") {
                newTeams.push(team);
            }
        }
        else {
            if (team.Nation == stat || stat == "All") {
                newTeams.push(team);
            }
        }
    });

    newTeams.sort((x, y) => x.Nation.localeCompare(y.Nation))

    let tbody = d3.select("#teams-list").select("tbody");

    tbody.selectAll("tr").remove();

    

    newTeams.forEach(team => {
        if (session != null) {
            if (favTeams.includes(team.TID)) {
                tbody.append("tr")
                    .html("<td id=\"" + team.TID + "\" style=\"cursor: pointer; color: yellow; text-shadow: 0 0 3px black; font-size: 30px;\" onclick=\"favoriteTeam(this)\">★</td>" +
                        "<td>" + team.Nation + "</td><td><img src=\"" + team.Badge + "\"></td><td class=\"align-middle\"><a href=\"/Team/Details?id=" + team.TID + "\">" + team.Teamname + "</a></td>");
            }
            else {
                tbody.append("tr")
                    .html("<td id=\"" + team.TID + "\" style=\"cursor: pointer; color: #dbdbdb; text-shadow: 0 0 3px black; font-size: 30px;\" onclick=\"favoriteTeam(this)\">★</td>" +
                        "<td>" + team.Nation + "</td><td><img src=\"" + team.Badge + "\"></td><td class=\"align-middle\"><a href=\"/Team/Details?id=" + team.TID + "\">" + team.Teamname + "</a></td>");
            }
        }
        else {
            tbody.append("tr")
                .html("<td>" + team.Nation + "</td><td><img src=\"" + team.Badge + "\"></td><td class=\"align-middle\"><a href=\"/Team/Details?id=" + team.TID + "\">" + team.Teamname + "</a></td>");
        }
        
    })

    fixImages();
});

function DisplayTeamsList() {
    let teamsTable = d3.select("#teams-table");

    var columns = [
        { head: "Teams", html: function (d) { return d.teamName } }
    ]

    $.get("https://localhost:5001/api/teams", function (data, status) {
        console.log("Team data:");
        console.log(data);
        var teamTable = teamsTable.append("table")

        teamsTable.append("thead").append("tr")
            .selectAll("th")
            .data(columns).enter()
            .append('th')
            .text((d) => d.head);

        teamsTable.append('tbody')
            .selectAll('tr')
            .data(data).enter()
            .append('tr')
            .selectAll('td')
            .data(function (row, i) {
                return columns.map(function (c) {
                    var cell = {};
                    d3.keys(c).forEach(function (k) {
                        cell[k] = typeof c[k] == 'function' ? c[k](row, i) : c[k];
                    });
                    return cell;
                });
            }).enter()
            .append('td')
            .html((d) => d.html)
    });
}

function fixImages() {
    var images = document.images;

    for (var i = 0; i < images.length; i++) {
        images[i].onerror = function () {
            this.src = "/images/default-team-image.png";
            this.width = 150;
        }
    }
}

function favoriteTeam(obj) {
    var favTeamsList = favTeams
    var star = document.getElementById(obj.id);
    var team = star.id;
    var userSession = session;
    console.log(favTeamsList);

    var data = { TID: team }

    if (favTeamsList.includes(parseInt(team))) {
        star.style.color = "#dbdbdb";

        $.ajax({
            type: "POST",
            url: "/Team/RemoveFavoriteTeam",
            data: data,

            success: function (data) {
                console.log("Removed fav team!");

                let index = favTeams.indexOf(team);
                favTeams.splice(index, 1);
            },
            complete: function () {
            },
            fail: function () {
            }
        });
    }
    else {
        star.style.color = "yellow"

        $.ajax({
            type: "POST",
            url: "/Team/AddFavoriteTeam",
            data: data,

            success: function (data) {
                console.log("Added fav team!");

                favTeams.push(parseInt(team));
            },
            complete: function () {
            },
            fail: function () {
            }
        });
    }
}