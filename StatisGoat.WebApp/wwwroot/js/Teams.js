console.log("data:");
console.log(data);

let width = 1800;
let height = 500;

// Create a Tooltip
var tooltip = d3.select("#match-visuals")
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

displayInitialPreviousMatchesStats();

fixImages();

function displayInitialPreviousMatchesStats() {
    var svg = d3.select("#previous-matches-svg");

    svg
        .attr("width", width)
        .attr("height", height);

    let statData = [];
    let xstatData = [];

    data.previousMatchesStats.forEach(function (match) {
        statData.push(match.Scored);
    });

    data.previousxMatchesStats.forEach(function (match) {
        xstatData.push(match.xGoals);
    });

    let avg = data.avgStats["Avgscored"];
    let avgxstat = data.previousxMatchesStats.reduce((r, d) => r + d["xGoals"], 0) / data.previousMatchesStats.length;
    let max = Math.max(d3.max(statData), d3.max(xstatData));
    if (xstatData.length == 0) {
        max = d3.max(statData);
    }
    if (statData.length > 0) {
        displayPreviousMatchesStats([0, max + 3], "Scored", "xGoals", statData, xstatData, avg, avgxstat);
    }

    d3.select("#sort-by-buttons").selectAll("button").on("click", function () {
        d3.select("#sort-by-buttons").selectAll("button").classed("active", false);
        let button = d3.select(this);
        button.classed("active", true);

        let stat = button["_groups"][0][0]["innerHTML"];
        let xstat = "x" + stat;
        xstat[1] = xstat[1].toUpperCase();
        let avgstat = "Avg" + stat.toLowerCase();
        let domain = [0, 1];
        let newStatData = [];
        let newxStatData = [];

        if (xstat == "xScored") {
            xstat = "xGoals";
        }

        for (let i = 0; i < data.previousMatchesStats.length; i++) {
            newStatData.push(data.previousMatchesStats[i][stat]);
            if (data.previousxMatchesStats.length > 0) {
                newxStatData.push(data.previousxMatchesStats[i][xstat]);
            }
        }

        let avgxstat = 0;

        if (newxStatData[0] != undefined) {
            avgxstat = newxStatData.reduce((r, d) => r + d, 0) / 10;
        }

        let max = d3.max(newStatData);
        if (newxStatData.length > 0 && newxStatData[0] != undefined) {
            max = Math.max(max, d3.max(newxStatData));
        }       

        switch (stat) {
            case "Scored":
                domain = [0, max + 3];
                break;
            case "Conceded":
                domain = [0, max + 3];
                break;
            case "Shots":
                domain = [0, max + 5];
                break;
            case "Fouls":
                domain = [0, max + 3];
                break;
            case "Corners":
                domain = [0, max + 3];
                break;
            case "Offsides":
                domain = [0, max + 3];
                break;
            case "Possession":
                domain = [0, max + 10];
                break;
            case "Saves":
                domain = [0, max + 3];
                break;
            case "Passes":
                domain = [0, max + 20];
                break;
            case "Pass %":
                domain = [0, 1];
                stat = "Pass_pct";
                avgstat = "Avgpass_pct";
                break;
        }

        if (newStatData.length > 0) {
            displayPreviousMatchesStats(domain, stat, xstat, newStatData, newxStatData, data.avgStats[avgstat], avgxstat);
        }
    });

}

function displayPreviousMatchesStats(domain, stat, xstat, statData, xStatData, avg, avgxstat) { // , predictionData) {
    var groups = ["real"];
    var group = [1];
    var svg = d3.select("#previous-matches-svg");

    svg.selectAll(".view").remove();
    svg.selectAll(".legend").remove();

    svg.append("circle").attr("cx", width - 200).attr("cy", 30).attr("r", 6).style("fill", "#CB0025").attr("class", "legend")
    svg.append("circle").attr("cx", width - 200).attr("cy", 60).attr("r", 6).style("fill", "#5C7AFF").attr("class", "legend")
    svg.append("text").attr("x", width - 180).attr("y", 30).text("Actual").style("font-size", "15px").attr("alignment-baseline", "middle").attr("class", "legend")
    svg.append("text").attr("x", width - 180).attr("y", 60).text("Prediction").style("font-size", "15px").attr("alignment-baseline", "middle").attr("class", "legend")
    let xdomain = [];

    for (let i = data.previousMatchesStats.length - 1; i >= 0; i--) {
        let dateString = data.previousMatchesStats[i].Datetime;
        let date = new Date(dateString);
        xdomain.push(date.toLocaleDateString("en-US"))
    }

    var x = d3.scalePoint()
        .domain(xdomain)
        .range([75, width-200]);

    svg.append("g")
        .attr("class", "view")
        .attr("transform", "translate(0," + (height - 25) + ")")
        .call(d3.axisBottom(x).tickSize(-height))
        .selectAll("line")
        .attr("opacity", ".4");

    let yAxis = svg.selectAll(".y-axis");

    var y = d3.scaleLinear()
        .domain(domain)
        .range([height - 40, 0]);

    y.nice();

    if (yAxis["_groups"][0].length == 0) {
        svg.append("g")
            .attr("class", "y-axis")
            .attr("transform", "translate(40,10)")
            .call(d3.axisLeft(y).ticks(6).tickSize(-width))
            .selectAll("line")
            .attr("opacity", ".4");
    }
    else {
        yAxis.transition()
            .duration(1000)
            .call(d3.axisLeft(y).ticks(6).tickSize(-width));
    }

    svg.selectAll("text")
        .style("font-size", "14px");

    let paths = svg.selectAll(".stat-line")["_groups"][0]

    if (paths == null || paths.length == 0) {
        svg.append("path")
            .datum(data.previousMatchesStats)
            .attr("class", "stat-line")
            .attr("transform", "translate(0, 10)")
            .attr("fill", "none")
            .attr("stroke", "#CB0025")
            .attr("stroke-width", 5)
            .attr("d", d3.line()
                .x(function (d) { let day = new Date(d.Datetime); return x(day.toLocaleDateString("en-US")) })
                .y(d => y(d[stat]))
            )
            .style("opacity", "1");

        svg.append("path")
            .datum(data.previousxMatchesStats)
            .attr("class", "xstat-line")
            .attr("transform", "translate(0, 10)")
            .attr("fill", "none")
            .attr("stroke", "#5C7AFF")
            .attr("stroke-width", 5)
            .attr("d", d3.line()
                .x(function (d) { let day = new Date(d.Datetime); return x(day.toLocaleDateString("en-US")) })
                .y(d => y(d[xstat]))
            )
            .style("opacity", "1");
    }
    else {
        let path = svg.select(".stat-line");
        path.transition()
            .duration(1000)
            .attr("d", d3.line()
                .x(function (d) { let day = new Date(d.Datetime); return x(day.toLocaleDateString("en-US")) })
                .y(d => y(d[stat])))
            .attr("transform", "translate(0, 10)")
            .attr("fill", "none")
            .attr("stroke", "#CB0025")
            .attr("stroke-width", 5)
            .style("opacity", "1");

        if (xStatData.length > 0 && xStatData[0] != undefined) {
            let xpath = svg.select(".xstat-line");
            xpath.transition()
                .duration(1000)
                .attr("d", d3.line()
                    .x(function (d) { let day = new Date(d.Datetime); return x(day.toLocaleDateString("en-US")) })
                    .y(d => y(d[xstat])))
                .attr("transform", "translate(0, 10)")
                .attr("fill", "none")
                .attr("stroke", "#5C7AFF")
                .attr("stroke-width", 5)
                .style("opacity", "1");
        }
        else {
            let xpath = svg.select(".xstat-line");
            xpath.transition()
                .duration(1000)
                .attr("d", d3.line()
                    .x(function (d) { let day = new Date(d.Datetime); return x(day.toLocaleDateString("en-US")) })
                    .y(d => y(0)))
                .attr("transform", "translate(0, 10)")
                .attr("fill", "none")
                .attr("stroke", "#5C7AFF")
                .attr("stroke-width", 5)
                .style("opacity", "0");
        }
    }

    let dots = svg.selectAll(".game-dots")["_groups"][0];
    if (dots == null || dots.length == 0) {
        // Game dots
        svg.append("g")
            .selectAll("dot")
            .data(data.previousMatchesStats)
            .enter()
            .append("circle")
            .attr("class", "game-dots")
            .attr("cx", function (d) { let day = new Date(d.Datetime); return x(day.toLocaleDateString("en-US")) })
            .attr("cy", function (d) { return y(d[stat]) })
            .attr("r", 5)
            .attr("transform", "translate(-1.5, 10)")
            .attr("fill", "white")
            .attr("stroke", "black")
            .attr("stroke-width", 2)
            .on("mouseover", (event, d) => {
                console.log(event)
                return tooltip
                    .style("visibility", "visible")
                    .html('<p>' + event.Teamname + ' Vs. ' + event.Opponentname + '</p><p>Result: ' + event.Result + '</p>')
            })
            .on("mousemove", function (event, d) {return tooltip.style("top", d3.event.pageY - 125 + "px").style("left", d3.event.pageX + 25 + "px"); })
            .on("mouseout", function () { return tooltip.style("visibility", "hidden"); });
    }
    else {
        dots = svg.selectAll(".game-dots");
        dots
            .data(data.previousMatchesStats)
            .enter()
            .append("circle")
            .merge(dots)
            .transition()
            .duration(1000)
            .attr("class", "game-dots")
            .attr("cx", function (d) { let day = new Date(d.Datetime); return x(day.toLocaleDateString("en-US")) })
            .attr("cy", function (d) { return y(d[stat]) })
            .attr("r", 5)
            .attr("transform", "translate(-1.5, 10)")
            .attr("fill", "white")
            .attr("stroke", "black")
            .attr("stroke-width", 2);
    }

    // Avg stat line
    let avgLine = svg.selectAll(".avg-stat-line");
    let avgxLine = svg.selectAll(".avg-xstat-line");
    if (avgLine["_groups"][0].length == 0) {
        // Avg stat line
        svg.append("g")
            .append("line")
            .attr("class", "avg-stat-line")
            .attr("x1", d => x(xdomain[0]) - 10)
            .attr("x2", d => x(xdomain[data.previousMatchesStats.length - 1]) + 10)
            .attr("y1", d => y(avg))
            .attr("y2", d => y(avg))
            .attr("stroke", "#CB0025")
            .attr("stroke-width", 2)
            .attr("stroke-dasharray", 3.5)
            .attr("transform", "translate(-1.5, 10)");
        if (xStatData.length > 0) {
            // Avg xstat line
            svg.append("g")
                .append("line")
                .attr("class", "avg-xstat-line")
                .attr("x1", d => x(xdomain[0]) - 10)
                .attr("x2", d => x(xdomain[data.previousxMatchesStats.length - 1]) + 10)
                .attr("y1", d => y(avgxstat))
                .attr("y2", d => y(avgxstat))
                .attr("stroke", "#5C7AFF")
                .attr("stroke-width", 2)
                .attr("stroke-dasharray", 3.5)
                .attr("transform", "translate(-1.5, 10)");

            // Avg xstat line text
            svg.append("g")
                .append("text")
                .attr("class", "xavg-text")
                .attr("x", d => x(xdomain[data.previousxMatchesStats.length - 1]) + 10)
                .attr("y", d => y(avgxstat) + 15)
                .text(Math.round(avgxstat * 10) / 10)
                .style("font-size", "14px");
        }

        // Avg stat line text
        svg.append("g")
            .append("text")
            .attr("class", "avg-text")
            .attr("x", d => x(xdomain[0])-40)
            .attr("y", d => y(avg) + 15)
            .text(Math.round(avg * 100) / 100)
            .style("font-size", "14px");       
    }
    else {
        // Avg stat line
        avgLine.transition()
            .duration(1000)
            .attr("class", "avg-stat-line")
            .attr("x1", d => x(xdomain[0]) - 10)
            .attr("x2", d => x(xdomain[data.previousMatchesStats.length-1]) + 10)
            .attr("y1", d => y(avg))
            .attr("y2", d => y(avg))
            .attr("stroke", "#CB0025")
            .attr("stroke-width", 2)
            .attr("stroke-dasharray", 3.5)
            .attr("transform", "translate(-1.5, 10)");

        if (xStatData.length > 0 && xStatData[0] != undefined) {
            // Avg xstat line
            avgxLine.transition()
                .duration(1000)
                .attr("class", "avg-xstat-line")
                .attr("x1", d => x(xdomain[0]) - 10)
                .attr("x2", d => x(xdomain[9]) + 10)
                .attr("y1", d => y(avgxstat))
                .attr("y2", d => y(avgxstat))
                .attr("stroke", "#5C7AFF")
                .attr("stroke-width", 2)
                .attr("stroke-dasharray", 3.5)
                .attr("transform", "translate(-1.5, 10)")
                .attr("opacity", "1");

            // Avg xstat line text
            svg.selectAll(".xavg-text")
                .transition()
                .duration(1000)
                .attr("x", d => x(xdomain[data.previousxMatchesStats.length - 1]) + 110)
                .attr("y", d => y(avgxstat) + 15)
                .text(Math.round(avgxstat * 10) / 10)
                .style("font-size", "14px")
                .attr("opacity", "1");
        }
        else {
            if (xStatData.length > 0) {
                // Avg xstat line
                avgxLine.transition()
                    .duration(1000)
                    .attr("class", "avg-xstat-line")
                    .attr("x1", d => x(xdomain[0]) - 10)
                    .attr("x2", d => x(xdomain[data.previousxMatchesStats.length - 1]) + 10)
                    .attr("y1", d => y(0))
                    .attr("y2", d => y(0))
                    .attr("stroke", "#5C7AFF")
                    .attr("stroke-width", 2)
                    .attr("stroke-dasharray", 3.5)
                    .attr("transform", "translate(-1.5, 10)")
                    .attr("opacity", "0");

                // Avg xstat line text
                svg.selectAll(".xavg-text")
                    .transition()
                    .duration(1000)
                    .attr("x", d => x(xdomain[data.previousxMatchesStats.length - 1]) + 10)
                    .attr("y", d => y(avgxstat) + 15)
                    .text(Math.round(avgxstat * 10) / 10)
                    .style("font-size", "14px")
                    .attr("opacity", "0");
            }
        }
        // Avg stat line text
        svg.selectAll(".avg-text")
            .transition()
            .duration(1000)
            .attr("x", d => x(xdomain[0]) - 35)
            .attr("y", d => y(avg) + 15)
            .text(Math.round(avg * 10) / 10)
            .style("font-size", "14px");    
    }
}

function fixImages() {
    var images = document.images;

    for (var i = 0; i < images.length; i++) {
        images[i].onerror = function () {
            if (this.className == "Team") {
                this.src = "/images/default-team-image.png";
                this.width = 200;
            }
            else if (this.className == "roster-image") {
                this.src = "/images/default-player-image.png";
                this.width = 120;
            }
            else if (this.className == "top-player-image") {
                this.src = "/images/default-player-image.png";
                this.width = 80;
            }
            else if (this.className == "previous-match-image") {
                this.src = "/images/default-team-image.png";
                this.width = 50;
            }
        }
    }
}