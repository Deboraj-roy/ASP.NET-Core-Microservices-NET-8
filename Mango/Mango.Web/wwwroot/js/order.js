var dataTable;

$(document).ready(function () {
    var params = new URLSearchParams(window.location.search);
    var status = params.get("status") || "all";  // fallback to "all"
    loadDataTable(status.toLowerCase());
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        order: [[0, 'desc']],
        ajax: { url: `/order/getall?status=${status}` },
        columns: [
            { data: 'orderHeaderId', width: "5%" },
            { data: 'email', width: "25%" },
            { data: 'name', width: "20%" },
            { data: 'phone', width: "10%" },
            { data: 'status', width: "10%" },
            { data: 'orderTotal', width: "10%" },
            {
                data: 'orderHeaderId',
                render: (data) => `
                    <div class="w-75 btn-group" role="group">
                        <a href="/order/orderDetail?orderId=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i>
                        </a>
                    </div>
                `,
                width: "10%"
            }
        ]
    });
}
