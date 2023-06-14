console.log("data: ");
console.log(data);

setupChart()

fixImages();

var tooltip = d3.select("#playerChartDiv")
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

function setupChart() {

    this.previousPlayerData = data.PreviousPlayerStats
    this.xPreviousPlayerData = data.xPreviousPlayerStats

    this.allData = [...this.previousPlayerData, ...this.xPreviousPlayerData]
    console.log(this.allData)
    this.padding = {
        left: 150,
        right: 150,
        top: 20,
        bottom: 100
    }

    this.chart = {
        width: 1000,
        height: 400
    }

    this.selection = "Goals"

    this.svg = d3.selectAll("#playerStatChart")
        .attr("width", chart.width)
        .attr("height", chart.height)

    this.scaleY = d3.scaleLinear()
        .range([chart.height-padding.bottom, padding.top])

    this.scaleX = d3.scalePoint()
        .domain(previousPlayerData.map((item) => { return item.Datetime.substring(0, 10) }))
        .range([padding.left, chart.width - padding.right])


    let sortingButtons = d3.select(".playerStatSorting").selectAll(".btn")

    sortingButtons
        .on("click", function (d,e) {
            updateChartData(e)
            d3.selectAll(".active").classed("active", false)
            d3.select(this).classed("active", true)
        })

    drawAxis()
    updateChartData(0)
}

function drawAxis(){

    this.y_axis = d3.axisLeft()
        .scale(this.scaleY)

    var x_axis = d3.axisBottom()
        .scale(this.scaleX)


    this.svg.append('text')
        .attr("x", this.chart.width/2)
        .attr("y", this.chart.height - this.padding.bottom/3)
        .text("Date")


    //legend: actual
    this.svg.append('text')
        .attr("x", this.chart.width-this.padding.right + 70)
        .attr("y", this.padding.top + 55)
        .text("Actual")

    this.svg.append('circle')
        .attr('cx', this.chart.width - this.padding.right + 60)
        .attr('cy', this.padding.top + 50)
        .attr('r', 5)
        .style('fill', '#CB0025');

    //legend: expected
    this.svg.append('text')
        .attr("x", this.chart.width - this.padding.right + 70)
        .attr("y", this.padding.top + 75)
        .text("Prediction")

    this.svg.append('circle')
        .attr('cx', this.chart.width - this.padding.right + 60)
        .attr('cy', this.padding.top + 70)
        .attr('r', 5)
        .style('fill', '#5C7AFF');

    this.svg.append("g")
        .attr('transform', 'translate(' + (this.padding.left) + ',0)')
        .attr("id", "y-axis")
        .call(d3.axisLeft(this.scaleY).tickPadding([0]).ticks(6).tickSize(-(this.chart.width - this.padding.right - this.padding.left)))
        .selectAll("line")
        .attr("opacity", ".5")

    this.svg.append("g")
        .attr('transform', 'translate(0,' + (this.chart.height - this.padding.bottom) + ')')
        .attr("id", "x-axis")
        .call(d3.axisBottom(this.scaleX).tickSize(-(this.chart.height - this.padding.bottom - this.padding.top)))
        .selectAll("text")
        .attr("transform", "rotate(-45)")
        .style("text-anchor", "end")

    this.svg.select("#x-axis").selectAll("line")
        .attr("opacity", ".5")

    this.svg.selectAll("path")
        .style("opacity", "1")

    this.svg.select("#y-axis").selectAll("text")
        .attr('transform', 'translate(-15,0)')

}


function updateChartData(i) {

    var data = []
    console.log(i)
    switch (i) {
        case 0: //Goals
            this.selection = "Goals"
            break;
        case 1: //Passes
            this.selection = "Passes"
            break;
        case 2: //Shots
            this.selection = "Shots"
            break;
        case 3: //Assists
            this.selection = "Assists"
            break;
        case 4: //Dribbles
            this.selection = "Dribbles"
            break;
        case 5: //Saves
            this.selection = "Saves"
            break;
    }
 
    data = this.previousPlayerData.map((d, i) => {
        return {
            x: d.Datetime.substring(0, 10),
            y: d[this.selection],
            xY: this.xPreviousPlayerData[i]["x" + this.selection.charAt(0).toUpperCase() + this.selection.slice(1)],
            result: d.Result,
            tName: d.Teamname,
            opName: d.Opponentname
        }
    })

    this.average = data.reduce((r, d) => r + d.y, 0) / data.length
    this.xAverage = data.reduce((r, d) => r + d.xY, 0) / data.length
    console.log(data)
    drawChart(data)
}

function drawChart(newData) {

    const allEqual = arr => arr.every(d => d === newData[0])
    if (allEqual(newData)) {
        let min = d3.min(newData, d => d.y);
        min = Math.min(min, d3.min(newData, d => d.xY));
        this.scaleY
            .domain([min, d3.max(newData, function (d) { return d.y })])
    }
    else {
        let min = d3.min(newData, d => d.y);
        min = Math.min(min, d3.min(newData, d => d.xY));
        let max = d3.max(newData, function (d) { return d.xY + 2 });
        this.scaleY
            .domain([min, Math.max(max, d3.max(newData, function (d) { return d.y+2}))])
    }

    if (newData.length > 0) {
        d3.select("#y-label").remove()
        this.svg.append("text")
            .attr("text-anchor", "middle")
            .attr("id", "y-label")
            .attr("y", this.padding.left / 4)
            .attr("x", this.chart.height * -.4)
            .attr("dy", ".75em")
            .attr("transform", "rotate(-90)")
            .text(this.selection)

        this.svg.select("#y-axis")
            .transition()
            .duration(1000)
            .call(d3.axisLeft(this.scaleY).tickPadding([0]).ticks(6).tickSize(-(this.chart.width - this.padding.right - this.padding.left)))
            .selectAll("line")
            .attr("opacity", ".5")

        this.svg.select("#y-axis").selectAll("text")
            .attr('transform', 'translate(-15,0)')

        var line = this.svg.selectAll(".playerChartLine")
            .data([newData], function (d) { return d.X })

        var xLine = this.svg.selectAll(".playerChartXLine")
            .data([newData], function (d) { return d.X })

        var avgLine = this.svg.selectAll(".playerChartAvgLine")
            .data([newData], function (d) { return d.X })

        var xAvgLine = this.svg.selectAll(".playerChartXAvgLine")
            .data([newData], function (d) { return d.X })

        xAvgLine
            .enter()
            .append("path")
            .attr("class", "playerChartXAvgLine")
            .style("opacity", 1)
            .merge(xAvgLine)
            .transition()
            .duration(1000)
            .attr("d", d3.line()
                .x((d) => { return this.scaleX(d.x); })
                .y((d) => { return this.scaleY(this.xAverage); }))
            .attr("fill", "none")
            .attr("stroke", "#5C7AFF")
            .attr("stroke-dasharray", "5 5")
            .attr("stroke-width", 2)

        avgLine
            .enter()
            .append("path")
            .attr("class", "playerChartAvgLine")
            .style("opacity", 1)
            .merge(avgLine)
            .transition()
            .duration(1000)
            .attr("d", d3.line()
                .x((d) => { return this.scaleX(d.x); })
                .y((d) => { return this.scaleY(this.average); }))
            .attr("fill", "none")
            .attr("stroke", "#CB0025")
            .attr("stroke-dasharray", "5 5")
            .attr("stroke-width", 2)

        xLine
            .enter()
            .append("path")
            .attr("class", "playerChartXLine")
            .style("opacity", 1)
            .merge(xLine)
            .transition()
            .duration(1000)
            .attr("d", d3.line()
                .x((d) => { return this.scaleX(d.x); })
                .y((d) => { return this.scaleY(d.xY); }))
            .attr("fill", "none")
            .attr("stroke", "#5C7AFF")
            .attr("stroke-width", 3)


        line
            .enter()
            .append("path")
            .attr("class", "playerChartLine")
            .style("opacity", 1)
            .merge(line)
            .transition()
            .duration(1000)
            .attr("d", d3.line()
                .x((d) => { return this.scaleX(d.x); })
                .y((d) => { return this.scaleY(d.y); }))
            .attr("fill", "none")
            .attr("stroke", "#CB0025")
            .attr("stroke-width", 5)

        var avgLabel = this.svg.selectAll(".playerChartAvgLabel")
            .data([newData], function (d) { return d.X })

        avgLabel
            .enter()
            .append("text")
            .attr("class", "playerChartAvgLabel")
            .style("fill", "#CB0025")
            .merge(avgLabel)
            .attr("x", (this.chart.width - this.padding.right + 10))
            .attr("dy", ".75em")
            .transition()
            .duration(1000)
            .attr("y", this.scaleY(this.average) - 8)
            .text(this.average)

        var xAvgLabel = this.svg.selectAll(".playerChartXAvgLabel")
            .data([newData], function (d) { return d.X })

        xAvgLabel
            .enter()
            .append("text")
            .attr("class", "playerChartXAvgLabel")
            .style("fill", "#5C7AFF")
            .merge(xAvgLabel)
            .attr("x", (this.padding.left - 75))
            .attr("dy", ".75em")
            .transition()
            .duration(1000)
            .attr("y", this.scaleY(this.xAverage) - 8)
            .text(Math.round(this.xAverage * 100) / 100)


        if (d3.selectAll(".playerChartCircles")["_groups"][0].length == 0) {
            this.svg.append("g")
                .selectAll("dot")
                .data(newData)
                .enter()
                .append("circle")
                .attr("r", 5)
                //.attr("transform", "translate(93, 0)")
                .attr("fill", "white")
                .attr("stroke", "black")
                .attr("stroke-width", 2)
                .attr("class", "playerChartCircles")
                .attr("cx", (d) => { return this.scaleX(d.x) })
                .attr("cy", (d) => { return this.scaleY(d.y) })
                .on("mouseover", (event, d) => {
                    console.log(event);
                    return tooltip
                        .style("visibility", "visible")
                        .html('<p>' + event.tName + ' Vs. ' + event.opName + '</p><p>Result: ' + event.result + '</p>')
                })
                .on("mousemove", function (event, d) { return tooltip.style("top", d3.event.pageY - 400 + "px").style("left", d3.event.pageX - 775 + "px"); })
                .on("mouseout", function () { return tooltip.style("visibility", "hidden"); });
        }
        else {
            this.svg.selectAll(".playerChartCircles")
                .data(newData)
                .transition()
                .duration(1000)
                .attr("cx", (d) => { return this.scaleX(d.x) })
                .attr("cy", (d) => { return this.scaleY(d.y) })
        }

    }
    else {
        this.svg.select(".no-data-text").remove();
        this.svg.append("text")
            .attr("class", "no-data-text")
            .text("No Data")
            .style("transform", "translate(42%, 45%)")
            .style("color", "D3D3D3")
            .style("font-weight", "bold")
            .style("font-size", "45px")
            .style("opacity", "40%");
    }

}

function fixImages() {
    var images = document.images;

    for (var i = 0; i < images.length; i++) {
        images[i].onerror = function () {
            this.src = "/images/default-player-image.png";
            this.width = 200;

            if (this.className == "team-image") {
                this.src = "/images/default-team-image.png";
                this.width = 125;
                this.height = 100;
            }
        }
    }
}