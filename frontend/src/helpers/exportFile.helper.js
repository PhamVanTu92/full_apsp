import ExcelJS from "exceljs";

export const exportExcelFile = async ({
    fileName = "export.xlsx",
    sheetName = "Sheet1",
    columns = [],
    data = [],
    headerStyle = true
}) => {
    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet(sheetName);

    // columns
    worksheet.columns = columns;

    // header style
    if (headerStyle) {
        const headerRow = worksheet.getRow(1);
        headerRow.font = { bold: true };

        headerRow.eachCell((cell) => {
            cell.fill = {
                type: "pattern",
                pattern: "solid",
                fgColor: { argb: "FFD3D3D3" }
            };

            cell.border = {
                top: { style: "thin" },
                left: { style: "thin" },
                bottom: { style: "thin" },
                right: { style: "thin" }
            };

            cell.alignment = { vertical: "middle", horizontal: "center" };
        });
    }

    // data rows
    data.forEach((row) => {
        worksheet.addRow(row);
    });

    // download
    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    });

    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;
    a.click();

    window.URL.revokeObjectURL(url);
};