$(document).ready(function () {

    loadOrders();

});

let currentSortBy = "Id";
let currentSortOrder = "asc";

// Sorting Click Event
$(document).on("click", ".sort-link", function (e) {
    e.preventDefault();

    let sortBy = $(this).data("column");
    let pageNumber = $(this).data("page");

    if (sortBy === currentSortBy) {
        currentSortOrder = currentSortOrder === "asc" ? "desc" : "asc";
    } else {
        currentSortBy = sortBy;
        currentSortOrder = "asc";
    }

    loadOrders(
        pageNumber,
        $("#ordersPerPage").val(),
        currentSortBy,
        currentSortOrder,
        $("#searchOrders").val().trim(),
        $("#statusFilter").val(),
        $("#dateRangeFilter").val(),
        $("#fromDate").val(),
        $("#toDate").val()
    );
});


function updateSortIcons() {
    $(".sort-icons .asc, .sort-icons .desc").removeClass("active");

    let activeIcon = currentSortOrder === "asc" ? ".asc" : ".desc";
    $(".sort-link[data-column='" + currentSortBy + "']").find(activeIcon).addClass("active");
}


$("#searchOrders").on("keyup", function () {
    clearTimeout(this.delay);
    this.delay = setTimeout(function () {
        searchOrder();
    }, 300);
});

function searchOrder() {
    let searchTerm = $("#searchOrders").val().trim();
    loadOrders(1, 5, "Id", "asc", searchTerm);
}

function loadOrders(pageNumber = 1, pageSize = 5, sortby = "Id", sortOrder = "asc", searchTerm = "", status = "All", dateRange = "All time", fromDate = "", toDate = "") {
    $.ajax({
        url: "/Orders/GetOrders",
        type: "GET",
        data: { pageNumber: pageNumber, pageSize: pageSize, sortby: sortby, sortOrder: sortOrder, searchTerm: searchTerm, status: status, dateRange: dateRange, fromDate: fromDate, toDate: toDate },
        success: function (response) {
            $("#orderTableList").html(response);
            $("#ordersPerPage").val(pageSize);
            $(".orderspagination-link").removeClass("active");
            $(".orderspagination-link[data-page='" + pageNumber + "']").addClass("active");
            updateSortIcons();
        },

        error: function () {
            toastr.error("Error fetching TaxesAndFees.");
        }
    });

}
// Handle pagination button clicks
$(document).on("click", ".orderspagination-link", function (e) {
    e.preventDefault();
    let pageNumber = $(this).data("page");
    let pageSize = $("#ordersPerPage").val();
    let searchTerm = $("#searchOrders").val().trim();
    let status = $("#statusFilter").val();
    let dateRange = $("#dateRangeFilter").val();
    let fromDate = $("#fromDate").val();
    let toDate = $("#toDate").val();

    loadOrders(pageNumber, pageSize, currentSortBy, currentSortOrder, searchTerm, status, dateRange, fromDate, toDate);
});

// Handle items per page dropdown
$(document).on("change", "#ordersPerPage", function () {
    let pageSize = $(this).val();
    let searchTerm = $("#searchOrders").val().trim();
    let status = $("#statusFilter").val();
    let dateRange = $("#dateRangeFilter").val();
    let fromDate = $("#fromDate").val();
    let toDate = $("#toDate").val();

    loadOrders(1, pageSize, currentSortBy, currentSortOrder, searchTerm, status, dateRange, fromDate, toDate);
});



// Trigger filtering when a filter input changes
$("#searchOrders, #statusFilter, #dateRangeFilter").on("change keyup", function () {
    applyFilters();
});

// When the "Search" button is clicked, apply filters
$(document).on("click","#searchBtn", function () {
    applyFilters();
});

// Clear filters when "Clear" button is clicked
$(".cancel-btn-color").on("click", function () {
    $("#searchOrders").val("");
    $("#statusFilter").val("All");
    $("#dateRangeFilter").val("All time");
    $("#fromDate").val("");
    $("#toDate").val("");
    applyFilters();
});



// Function to apply filters
function applyFilters() {
    let searchTerm = $("#searchOrders").val().trim();
    let status = $("#statusFilter").val();
    let dateRange = $("#dateRangeFilter").val();
    let fromDate = $("#fromDate").val();
    let toDate = $("#toDate").val();
    loadOrders(1, $("#ordersPerPage").val(), currentSortBy, currentSortOrder, searchTerm, status, dateRange, fromDate, toDate);
}

$(document).on("click", "#exportBtn", function () {
    $("#exportBtn").prop("disabled", true);
    $("#fullscreenLoader").css("display", "flex");

    var status = $("#statusFilter").val() || "";
    var date = $("#dateRangeFilter").val() || "";
    var searchText = $("#searchOrders").val() || "";

    var url = `/Orders/ExportOrders?status=${encodeURIComponent(status)}&date=${encodeURIComponent(date)}&searchText=${encodeURIComponent(searchText)}`;

    var xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.responseType = "blob";

    xhr.onload = function () {
        if (xhr.status === 200) {
            var blob = new Blob([xhr.response], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
            var link = document.createElement("a");
            link.href = window.URL.createObjectURL(blob);
            link.download = "Orders.xlsx";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            toastr.success("Excel file exported successfully!");
        } else {
            toastr.error("Failed to export file.");
        }

        $("#fullscreenLoader").hide();
        $("#exportBtn").prop("disabled", false);
    };

    xhr.onerror = function () {
        toastr.error("Error occurred during export.");
        $("#fullscreenLoader").hide();
        $("#exportBtn").prop("disabled", false);
    };

    xhr.send();
});


$(document).ready(function () {
    $(document).on("click", ".generateInvoice", function () {
        var orderId = $(this).data("id");
        if (!orderId) {
            toastr.error("Invalid order ID.");
            return;
        }

        var downloadUrl = `/Orders/GenerateInvoicePdf/${orderId}`;

        $("#fullscreenLoader").css("display", "flex");

        fetch(downloadUrl)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Download failed");
                }
                return response.blob();
            })
            .then(blob => {
                const link = document.createElement("a");
                link.href = URL.createObjectURL(blob);
                link.download = `Invoice_${orderId}.pdf`;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                toastr.success("Invoice downloaded successfully.");
            })
            .catch(error => {
                console.error(error);
                toastr.error("Error downloading invoice.");
            })
            .finally(() => {
                $("#fullscreenLoader").fadeOut();
            });
    });
});


$(document).on("click", ".orderDetailsPdf", function () {
    var orderId = $(this).data("id");
    if (!orderId) {
        toastr.error("Invalid order ID.");
        return;
    }

    var downloadUrl = `/Orders/GenerateOrderDetailsPdf/${orderId}`;

    $("#fullscreenLoader").css("display", "flex");

    fetch(downloadUrl)
        .then(response => {
            if (!response.ok) {
                throw new Error("Download failed");
            }
            return response.blob();
        })
        .then(blob => {
            const link = document.createElement("a");
            link.href = URL.createObjectURL(blob);
            link.download = `OrderDetails_${orderId}.pdf`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            toastr.success("OrderDetails downloaded successfully.");
        })
        .catch(error => {
            console.error(error);
            toastr.error("Error downloading OrderDetials.");
        })
        .finally(() => {
            $("#fullscreenLoader").fadeOut();
        });
});






