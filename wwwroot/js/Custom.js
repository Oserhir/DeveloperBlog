let index = 0;

function AddTag() {
    // Get a referene to the TagEntry input element
    var tagEntry = document.getElementById("TagEntry");

    // Use Search function to detect an error state
    let searchResult = Search(tagEntry.value);

    if (searchResult != null) {

        // trigger sweet alert for the error
        swalWithDarkButton.fire({
            html: `<span class='font-weight-bolder'>${searchResult.toUpperCase()}</span>`
        });
    }
    else {

        // Create a new Select Option
        let newOption = new Option(tagEntry.value, tagEntry.value);
        document.getElementById("TagList").options[index++] = newOption;
    }

    // Clear out the TagEntry control
    tagEntry.value = "";

    return true;
}

function DeleteTag() {

    let tagCount = 1;
    let tagList = document.getElementById("TagList");

    if (!tagList) return false;

    if (tagList.selectedIndex == -1) {
        swalWithDarkButton.fire({
            html: "<span class='font-weight-bolder'>CHOOSE A TAG BEFORE DELETING!</span>"
        })
        return true;
    }

    while (tagCount > 0) {

        if (tagList.selectedIndex >= 0) {
            tagList.options[tagList.selectedIndex] = null;
            --tagCount
        }
        else {
            tagCount = 0;
        }
        index--;
    }
}

function ValidateComment() {
    var message = document.getElementById("Message");

    // Check if message is empty or has only white spaces
    if (message.innerText.length === 0 || message.innerText.trim()) {
        //Swal.fire('ENTER A MESSAGE!');
        //swalWithDarkButton.fire({
        //    html: `<span class='font-weight-bolder'>ENTER A MESSAGE!</span>`
        //});
        //return true;
    }
}

// This selects all tags on submition to allow them to be stored, if they're not selected nothing will be stored
$("form").on("submit", function () {
    $("#TagList option").prop("selected", "selected");
})

// Look for TagValues variable to see if it has data
if (tagValues != '') {
    let tagArray = tagValues.split(",");
    for (var i = 0; i < tagArray.length; i++) {
        // load up or replace the options that we have
        ReplaceTag(tagArray[i], i);
        index++;
    }
}

function ReplaceTag(tag, index) {
    let newOption = new Option(tag, tag);
    document.getElementById("TagList").options[index] = newOption;
}

// The search function will detect an empty or duplicate Tag and return an error string if error is detected
function Search(str) {
    if (str == "") {
        return 'Empty tags are not permitted';
    }

    var tagsEl = document.getElementById("TagList");
    if (tagsEl) {
        let options = tagsEl.options;
        for (var i = 0; i < options.length; i++) {
            if (options[i].value == str) {
                return `The Tag #${str} is a duplicate and not permitted`;
            }
        }
    }
}

const swalWithDarkButton = Swal.mixin({
    customClass: {
        confirmButton: 'btn btn-sm btn-block btn-danger btn-outline-dark'
    },
    imageUrl: '/img/attention.jpg',
    timer: 5000,
    buttonsStyling: false
});