$(document).ready(function () {

    loadCustomers();

});

let currentSortBy = "Date";
let currentSortOrder = "desc";

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

    let dateRange = $("#dateRangeFilterCustomer").val();
    let startDate = $("#startDate").val();
    let endDate = $("#endDate").val();

    loadCustomers(
        pageNumber,
        $("#customersPerPage").val(),
        currentSortBy,
        currentSortOrder,
        $("#searchCustomers").val().trim(),
        dateRange,
        startDate,
        endDate
    );
});


// sort icon update
function updateSortIcons() {
    $(".sort-icons .asc, .sort-icons .desc").removeClass("active");
    let activeIcon = currentSortOrder === "asc" ? ".asc" : ".desc";
    $(".sort-link[data-column='" + currentSortBy + "']").find(activeIcon).addClass("active");
}

// search customer
$("#searchCustomers").on("keyup", function () {
    clearTimeout(this.delay);
    this.delay = setTimeout(function () {
        searchCustomers();
    }, 300);
});

function searchCustomers() {
    let searchTerm = $("#searchCustomers").val().trim();
    loadCustomers(1, 5, "Date", "desc", searchTerm);
}

// Fetch Customer Function
function loadCustomers(pageNumber = 1, pageSize = 5, sortby = "Date", sortOrder = "desc", searchTerm = "", dateRange = "All time", startDate = null, endDate = null) {
    $.ajax({
        url: "/Customers/GetCustomers",
        type: "GET",
        data: { pageNumber: pageNumber, pageSize: pageSize, sortby: sortby, sortOrder: sortOrder, searchTerm: searchTerm, dateRange: dateRange, customStartDate: startDate, customEndDate: endDate },
        success: function (response) {
            $("#customerTableList").html(response);
            $("#customersPerPage").val(pageSize);
            $(".customerspagination-link").removeClass("active");
            $(".customerspagination-link[data-page='" + pageNumber + "']").addClass("active");
            updateSortIcons();
        },

        error: function () {
            toastr.error("Error fetching TaxesAndFees.");
        }
    });

}
// Handle pagination button clicks
$(document).on("click", ".customerspagination-link", function (e) {
    e.preventDefault();
    let pageNumber = $(this).data("page");
    let pageSize = $("#customersPerPage").val();
    let searchTerm = $("#searchCustomers").val().trim();
    let dateRange = $("#dateRangeFilterCustomer").val();
    let startDate = $("#startDate").val();
    let endDate = $("#endDate").val();

    loadCustomers(pageNumber, pageSize, currentSortBy, currentSortOrder, searchTerm, dateRange, startDate, endDate);
});

// Handle items per page dropdown
$(document).on("change", "#customersPerPage", function () {
    let pageSize = $(this).val();
    let searchTerm = $("#searchCustomers").val().trim();
    let dateRange = $("#dateRangeFilterCustomer").val();
    let startDate = $("#startDate").val();
    let endDate = $("#endDate").val();


    loadCustomers(1, pageSize, currentSortBy, currentSortOrder, searchTerm, dateRange, startDate, endDate);
});


// Trigger filtering when a filter input changes
$("#searchCustomers, #dateRangeFilterCustomer").on("change keyup", function () {
    applyFilters();
});


//filter function
function applyFilters(startDate = null, endDate = null) {
    let searchTerm = $("#searchCustomers").val().trim();
    let dateRange = $("#dateRangeFilterCustomer").val();


    if (dateRange === "Custom Date" && (!startDate || !endDate)) {
        return;
    }

    loadCustomers(
        1,
        $("#customersPerPage").val(),
        currentSortBy,
        currentSortOrder,
        searchTerm,
        dateRange,
        startDate,
        endDate
    );
}

// Date Range Filter modal show
$("#dateRangeFilterCustomer").on("change", function () {
    let dateRange = $(this).val();
    if (dateRange === "Custom Date") {
        $('#customDateModal').modal('show');
    } else {
        applyFilters();
    }
});

// daterange filter
$(document).on("click", "#applyDateRange", function () {
    let startDate = $("#startDate").val();
    let endDate = $("#endDate").val();

    if (!startDate || !endDate) {
        toastr.warning("Please select both start and end dates.");
        return;
    }

    const start = new Date(startDate);
    const end = new Date(endDate);

    if (start > end) {
        toastr.error("Start date cannot be after end date.");
        return;
    }

    $("#dateRangeFilterCustomer").val("Custom Date");
    $("#customDateModal").modal("hide");
    applyFilters(startDate, endDate);
});


// get customer history
// Listen for clicks on any row with class 'customer-row'
$(document).on("click", ".customer-row", function () {
    var customerId = $(this).data("id");

    if (!customerId) {
        alert("Invalid customer selected.");
        return;
    }
    fetchCustomerHistory(customerId);
});


//fetchcustomer history function
function fetchCustomerHistory(customerId) {
    $.ajax({
        url: "/Customers/GetCustomerHistory",
        type: "GET",
        data: { customerId: customerId },
        success: function (response) {
            if (response) {
                updateCustomerHistoryModal(response);
                $("#customerHistoryModal").modal("show");
            }
        },
        error: function () {
            alert("Failed to load customer history.");
        }
    });
}

function updateCustomerHistoryModal(data) {
    
    $("#customerName").text(data.name);
    $("#averageBill").text(data.averageBill.toFixed(2));
    $("#customerPhone").text(data.phoneNumber);
    $("#comingSince").text(data.comingSince);
    $("#maxOrder").text(data.maxOrder.toFixed(2));
    $("#visitCount").text(data.visits);

    var orderTable = $("#customerOrders tbody");
    orderTable.empty();
    $.each(data.orders, function (index, order) {
        orderTable.append(`
            <tr>
                <td>${order.date}</td>
                <td>${order.orderType}</td>
                <td>${order.payment}</td>
                <td>${order.items}</td>
                <td>${order.amount.toFixed(2)}</td>
            </tr>
        `);
    });
}

// export functionality
$(document).on("click", "#exportBtnCustomer", function () {
    $("#exportBtnCustomer").prop("disabled", true);
    $("#fullscreenLoader").css("display", "flex");

    var dateRange = $("#dateRangeFilterCustomer").val() || "";
    var searchTerm = $("#searchCustomers").val() || "";
    let customStartDate = $("#startDate").val();
    let customEndDate = $("#endDate").val();

    var url = `/Customers/ExportCustomer?dateRange=${encodeURIComponent(dateRange)}&searchTerm=${encodeURIComponent(searchTerm)}&customStartDate=${encodeURIComponent(customStartDate)}&customEndDate=${encodeURIComponent(customEndDate)}`;

    var xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.responseType = "blob";

    xhr.onload = function () {
        if (xhr.status === 200) {
            var blob = new Blob([xhr.response], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
            var link = document.createElement("a");
            link.href = window.URL.createObjectURL(blob);
            link.download = "Customer.xlsx";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            toastr.success("Excel file exported successfully!");
        } else {
            toastr.error("Failed to export file.");
        }

        $("#fullscreenLoader").hide();
        $("#exportBtnCustomer").prop("disabled", false);
    };

    xhr.onerror = function () {
        toastr.error("Error occurred during export.");
        $("#fullscreenLoader").hide();
        $("#exportBtnCustomer").prop("disabled", false);
    };

    xhr.send();
});