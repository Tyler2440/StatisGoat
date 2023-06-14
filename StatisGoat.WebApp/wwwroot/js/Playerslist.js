$(document).ready(function () {
    fixImages();
    $("#spinner").show();
    $('#players-list').DataTable({
        scrollY: '50vh',
        scrollCollapse: true,
        paging: false,
    });
    $("#spinner").hide()
    fixImages();
})



function fixImages() {
    var images = document.images;

    for (var i = 0; i < images.length; i++) {
        images[i].onerror = function () {
            if (this.className == "Team") {
                this.src = "/images/default-team-image.png";
                this.width = 75;
            }
            else {
                this.src = "/images/default-player-image.png";
                this.width = 125;
            }          
        }
    }
}