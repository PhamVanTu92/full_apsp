<template>
    <div> 
        <Dialog v-model:visible="visible" class="w-30rem" :header="t('client.export_files')" modal>
            <div class="flex flex-column gap-3 w-full p-2">
                <div class="field">
                    <label class="font-medium mb-2 block">{{ t('client.payment_method') }}</label>
                    <Dropdown class="w-full" v-model="selectedFileType" :options="paymentTypes" optionLabel="name" :placeholder="t('client.choose_payment_method')"></Dropdown>
                </div>

                <div class="field">
                    <label class="font-medium mb-2 block">{{ t('client.format') }}</label>
                    <div class="flex gap-4 p-1 border-1 border-round surface-border">
                        <div  class="format-option p-2 border-round-lg" :class="{ selected: selectedFormat === 'pdf' }" @click="selectedFormat = 'pdf'">
                            <div class="flex align-items-center gap-2">
                                <RadioButton v-model="selectedFormat" inputId="pdf" name="format" value="pdf" />
                                <label for="pdf" class="font-medium cursor-pointer flex align-items-center gap-2">
                                    <i class="pi pi-file-pdf text-red-500 text-xl"></i>
                                    <span>PDF</span>
                                </label>
                            </div>
                        </div>

                        <div class="format-option p-2 border-round-lg" :class="{ selected: selectedFormat === 'excel' }" @click="selectedFormat = 'excel'">
                            <div class="flex align-items-center gap-2">
                                <RadioButton v-model="selectedFormat" inputId="excel" name="format" value="excel" />
                                <label for="excel" class="font-medium cursor-pointer flex align-items-center gap-2">
                                    <i class="pi pi-file-excel text-green-500 text-xl"></i>
                                    <span>EXCEL</span>
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <template #footer>
                <div class="flex gap-2 w-full">
                    <Button :label="t('client.cancel')" @click="visible = false" severity="secondary" class="flex-grow-1" outlined/>
                    <Button :label="t('client.confirm')" @click="ExportData" class="flex-grow-1" icon="pi pi-download"/>
                </div>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useOrderDetailStore } from '../store/orderDetail';   
import { useOrderDetailStore as useOrderDetailStoreNET } from '../../../client/pages/user_menu/components/store/orderDetailNET';  
 
import { useToast } from 'primevue/usetoast';
import ExcelJS from 'exceljs';
import _ from 'lodash';
import { format } from 'date-fns';
import { fnumInt } from '../../PurchaseOrder/script';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const odStore = useOrderDetailStore();
const toast = useToast();
const visible = ref(false);
const selectedFormat = ref('excel');
const selectedFileType = ref(null); 
const paymentTypes = ref([
    { name: t('client.pay_immediately'), code: 'order' },
    { name: t('client.bank_transfer'), code: 'invoice' }
]);

const open = () => {
    visible.value = true;
};
defineExpose({ open });
const props = defineProps(
    {
        data: {
            type: Boolean,
            default: false
        },
        type: {
            type: String,
            default: 'ORDER'
        }
    }
);
const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});

const toastService = (severity: 'error' | 'secondary' | 'success' | 'info' | 'contrast' | 'warn', smr: string, detail: string, life: number = 3000) => {
    toast.add({
        severity: severity,
        detail: detail,
        summary: smr,
        life: life
    });
};
const ExportData = async () => {
    switch (selectedFormat.value) {
        case 'pdf':
            return ExportPDF();
        case 'excel':
            return ExportExcel();
    }

    // Xử lý xuất file tại đây
    toastService('success', 'Thành công', `Đã xuất file dạng ${selectedFormat.value.toUpperCase()}`, 3000);
    visible.value = false;
};
const addFontsToPDF = (doc: any) => {
    doc.addFont('/fonts/times-new-roman-normal.ttf', 'Times New Roman', 'normal');
    doc.addFont('/fonts/times-new-roman-bold.ttf', 'Times New Roman', 'bold');
    doc.addFont('/fonts/times-new-roman-italic.ttf', 'Times New Roman', 'italic');
    doc.addFont('/fonts/times-new-roman-bold-italic.ttf', 'Times New Roman', 'bolditalic');
    doc.setFont('Times New Roman');
};
const addressDefault = {
    address: 'Văn phòng: Lầu 1, 6B Tôn Đức Thắng, Phường Sài Gòn, Thành phố Hồ Chí Minh, Việt Nam',
    locationName: 'Nhà máy: 990 Nguyễn Thị Định, Phường Cát Lái, Thành phố Hồ Chí Minh, Việt Nam',
    vat: '8%'
};
const ExportPDF = async () => {
    try {  
        const odStore = props.type == "NET" ? useOrderDetailStoreNET() : useOrderDetailStore();
        const { jsPDF } = await import('jspdf');
        const autoTable = (await import('jspdf-autotable')).default;

        const doc = new jsPDF('p', 'mm', 'a4');
        addFontsToPDF(doc);
        doc.setFont('Times New Roman');

        // Load logo
        try {
            const response = await fetch('/image/logo/new-logo-ap.png');
            const imageBlob = await response.blob();
            const reader = new FileReader();
            const base64Logo = await new Promise<string>((resolve) => {
                reader.onloadend = () => {
                    const result = reader.result as string;
                    resolve(result || '');
                };
                reader.readAsDataURL(imageBlob);
            });

            if (base64Logo) {
                doc.addImage(base64Logo, 'PNG', 150, 8, 45, 15);
            }
        } catch (error) {

        }

        // Tiêu đề công ty
        doc.setFontSize(10);
        doc.setFont('Times New Roman', 'bolditalic');
        doc.text('Công Ty Cổ phần AP Saigon Petro', 10, 18);

        doc.setFont('Times New Roman', 'italic');
        doc.text(addressDefault.address, 10, 23);
        doc.text(addressDefault.locationName, 10, 28);

        // Đường kẻ phân cách
        doc.setDrawColor(0, 0, 0);
        doc.setLineWidth(0.5);
        doc.line(10, 32, 200, 32);

        // Tiêu đề chính
        doc.setFontSize(16);
        doc.setFont('Times New Roman', 'bold');
        doc.text('ĐƠN ĐẶT HÀNG', 105, 42, { align: 'center' });

        doc.setFontSize(11);
        doc.setFont('Times New Roman', 'italic');
        doc.text('( Có giá trị như giấy đề nghị bán hàng, giấy đề nghị xuất hóa đơn, lệnh giao hàng)', 105, 48, { align: 'center' });

        // Thông tin khách hàng
        doc.setFontSize(11);
        doc.setFont('Times New Roman', 'bolditalic');

        let y = 58;
        doc.text('Khách hàng: ', 10, y);
        doc.text(odStore.order?.cardName || '', 35, y);

        y += 7;
        doc.text('Địa chỉ: ', 10, y);
        const address = odStore.order?.address ? `${odStore.order?.address[0].address || ''}, ${odStore.order?.address[0].locationName || ''}, ${odStore.order?.address[0].areaName || ''}` : '';
        doc.text(address, 25, y);

        y += 7;
        doc.text('MST: ', 10, y);
        doc.text(odStore.customer?.licTradNum || '', 25, y);

        y += 7;
        doc.text('Đơn hàng số: ', 10, y);
        doc.text(odStore.order?.invoiceCode || '', 35, y);

        y += 7;
        doc.text('Ngày giao hàng: ', 10, y);
        doc.text(format(odStore.order?.deliveryTime || new Date(), 'HH:mm - dd/MM/yyyy'), 40, y);
        y += 7;
        doc.text('Ngày đặt hàng: ', 10, y);
        doc.text(format(odStore.order?.docDate || new Date(), 'HH:mm - dd/MM/yyyy'), 40, y);
        y += 10;
        // Thêm khoảng cách trước bảng

        // Bảng sản phẩm
        const tableData: any[][] = [];
        _.forEach(odStore.order?.itemDetail, (item, index) => {
            tableData.push([
                index + 1,
                (item as any).itemName || '',
                (item as any).itemCode || '',
                (item as any).uomName || (item as any).unitName || '',
                (item as any).quantity || 0,
                fnumInt((item as any).priceAfterDist || (item as any).unitCost || 0),
                fnumInt(((item as any).quantity || 0) * ((item as any).priceAfterDist || (item as any).unitCost || 0))
            ]);
        });

        autoTable(doc, {
            head: [['STT', 'Tên sản phẩm', 'Mã hàng', 'ĐVT', 'Số lượng', 'Đơn giá', 'Thành tiền']],
            body: tableData,
            startY: y,
            margin: { left: 10, right: 10 },
            styles: {
                font: 'Times New Roman',
                fontSize: 9,
                cellPadding: 3,
                lineColor: [0, 0, 0],
                lineWidth: 0.5
            },
            headStyles: {
                fillColor: [198, 225, 181],
                textColor: [0, 0, 0],
                fontStyle: 'bold',
                lineColor: [0, 0, 0],
                lineWidth: 0.5,
                minCellHeight: 8
            },
            bodyStyles: {
                lineColor: [0, 0, 0],
                lineWidth: 0.5,
                minCellHeight: 6
            },
            columnStyles: {
                0: { halign: 'center', cellWidth: 15 },
                1: { cellWidth: 50 },
                2: { cellWidth: 30 },
                3: { halign: 'center', cellWidth: 20 },
                4: { halign: 'center', cellWidth: 20 },
                5: { halign: 'right', cellWidth: 25 },
                6: { halign: 'right', cellWidth: 30 }
            }
        });

        // Tính toán vị trí sau bảng
        const finalY = (doc as any).lastAutoTable ? (doc as any).lastAutoTable.finalY + 8 : y + 50;
        doc.text('Sản Phẩm Khuyến Mãi + Vật Phẩm Khuyến Mãi ( Nếu có ) ', 10, finalY);

        // Bảng khuyến mãi (nếu có)
        const itemPromotion: any[][] = [];
        _.forEach(odStore.order?.promotion, (item, index) => {
            if (item.quantityAdd > 0)
                // Bỏ qua nếu không có số lượng khuyến mãi
                itemPromotion.push([index + 1, (item as any).itemName || '', (item as any).itemCode || '', (item as any).packingName || '', (item as any).quantityAdd || 0, '-', '-']);
        });

        let currentY = finalY + 5;

        if (itemPromotion.length > 0) {
            autoTable(doc, {
                head: [['STT', 'Tên sản phẩm', 'Mã hàng', 'ĐVT', 'Số lượng', 'Đơn giá', 'Thành tiền']],
                body: itemPromotion,
                startY: currentY,
                margin: { left: 10, right: 10 },
                styles: {
                    font: 'Times New Roman',
                    fontSize: 9,
                    cellPadding: 3,
                    lineColor: [0, 0, 0],
                    lineWidth: 0.5
                },
                headStyles: {
                    fillColor: [198, 225, 181],
                    textColor: [0, 0, 0],
                    fontStyle: 'bold',
                    lineColor: [0, 0, 0],
                    lineWidth: 0.5,
                    minCellHeight: 8
                },
                bodyStyles: {
                    lineColor: [0, 0, 0],
                    lineWidth: 0.5,
                    minCellHeight: 6
                },
                columnStyles: {
                    0: { halign: 'center', cellWidth: 15 },
                    1: { cellWidth: 50 },
                    2: { cellWidth: 30 },
                    3: { halign: 'center', cellWidth: 20 },
                    4: { halign: 'center', cellWidth: 20 },
                    5: { halign: 'right', cellWidth: 25 },
                    6: { halign: 'right', cellWidth: 30 }
                }
            });
            currentY = (doc as any).lastAutoTable ? (doc as any).lastAutoTable.finalY + 8 : currentY + 20;
        }
        // Kiểm tra xem có cần trang mới không
        if (currentY > 250) {
            doc.addPage();
            currentY = 20;
        }
        // Thông tin thanh toán
        doc.setFont('Times New Roman', 'bold');
        doc.setFontSize(10);

        doc.text('Tiền hàng trước chiết khấu:', 10, currentY);
        doc.text(fnumInt(odStore.order?.paymentInfo?.totalBeforeVat || 0), 200, currentY, { align: 'right' });

        currentY += 6;
        doc.text('Chiết khấu đơn hàng:', 10, currentY);
        doc.text(fnumInt(odStore.order?.paymentInfo?.bonusCommited || 0), 200, currentY, { align: 'right' });

        currentY += 6;
        doc.text('Tiền hàng sau chiết khấu:', 10, currentY);
        doc.text(fnumInt((odStore.order?.paymentInfo?.totalBeforeVat || 0) - (odStore.order?.paymentInfo?.bonusCommited || 0)), 200, currentY, { align: 'right' });

        currentY += 6;
        doc.text('VAT ' + addressDefault.vat + ':', 10, currentY);
        doc.text(fnumInt(odStore.order?.paymentInfo?.vatAmount || 0), 200, currentY, { align: 'right' });

        currentY += 6;
        doc.text('Tổng hoá đơn:', 10, currentY);
        doc.text(fnumInt((odStore.order?.paymentInfo?.totalBeforeVat || 0) + (odStore.order?.paymentInfo?.vatAmount || 0)), 200, currentY, { align: 'right' });

        currentY += 6;
        doc.text('Thưởng thanh toán:', 10, currentY);
        doc.text(fnumInt(odStore.order?.paymentInfo?.bonusAmount || 0), 200, currentY, { align: 'right' });

        currentY += 6;
        doc.text('Tổng thanh toán:', 10, currentY);
        doc.text(fnumInt((odStore.order?.paymentInfo?.totalAfterVat || 0) - (odStore.order?.paymentInfo?.bonusAmount || 0)), 200, currentY, { align: 'right' });

        // Thông tin bổ sung
        currentY += 15;
        doc.setFont('Times New Roman', 'bolditalic');
        doc.setFontSize(10);

        doc.text(`* Tổng số lít: ${odStore.order?.itemDetail.reduce((sum, item) => (item as any).itemVolume * (item as any).quantity + sum, 0)} (lít)`, 10, currentY);

        currentY += 6;
        doc.text(`* Số xe nhận hàng: ${shipAddress.value?.vehiclePlate || '-'}`, 10, currentY);
        doc.text(`* Tên người nhận hàng: ${shipAddress.value?.person || '-'}`, 105, currentY);

        currentY += 6;
        doc.text('* Hình thức thanh toán:', 10, currentY);
        doc.text('* Thời gian thanh toán:', 105, currentY);

        currentY += 6;
        doc.text('* Ghi chú:', 10, currentY);

        currentY += 9;
        doc.text('Công Ty Cổ Phần AP Saigon Petro', 115, currentY);
        currentY += 10;
        doc.text('Xác nhận bởi P. DVCSKH', 115, currentY);
        currentY += 11;
        doc.text(`Thời gian xác nhận:${odStore.order?.confirmAt ? format(odStore.order.confirmAt, 'HH:mm - dd/MM/yyyy') : ''}`, 115, currentY);
        // Xuất file
        const now = new Date();
        const fileName = `Đơn_hàng_${now.getDate()}-${now.getMonth() + 1}-${now.getFullYear()}_${now.getHours()}-${now.getMinutes()}.pdf`;
        doc.save(fileName);

        toastService('success', 'Thành công', 'Đã xuất file PDF', 3000);
        visible.value = false;
    } catch (error) {
        console.error('Lỗi xuất PDF:', error);
        toastService('error', 'Lỗi', 'Không thể xuất file PDF', 3000);
    }
};

const ExportExcel = async () => {
    try {
        if (_.isEmpty(odStore.order)) {

            return;
        }
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('Đơn đặt hàng');

        // Tiêu đề chính
        const titleRow = worksheet.addRow(['Công Ty Cổ phần AP Saigon Petro']);
        titleRow.font = { name: 'Times New Roman', size: 10, bold: true, italic: true };
        worksheet.mergeCells('A1:G1');
        const titleRow2 = worksheet.addRow([addressDefault.address]);
        titleRow2.font = { name: 'Times New Roman', size: 10, italic: true };
        worksheet.mergeCells('A2:G2');

        const titleRow3 = worksheet.addRow([addressDefault.locationName]);
        titleRow3.font = { name: 'Times New Roman', size: 10, italic: true };
        worksheet.mergeCells('A3:G3');

        const response = await fetch('/image/logo/new-logo-ap.png');
        const imageBlob = await response.blob();
        const reader = new FileReader();
        const base64Logo = await new Promise<string>((resolve) => {
            reader.onloadend = () => {
                const result = reader.result as string;
                resolve(result ? result.split(',')[1] : '');
            };
            reader.readAsDataURL(imageBlob);
        });

        const logoId = workbook.addImage({
            base64: base64Logo,
            extension: 'png'
        });
        worksheet.addImage(logoId, {
            tl: { col: 5, row: 1 },
            ext: { width: 220, height: 65 }
        });
        const mainTitle = worksheet.addRow(['ĐƠN ĐẶT HÀNG']);
        mainTitle.font = { name: 'Times New Roman', size: 16, bold: true };
        mainTitle.alignment = { horizontal: 'center', vertical: 'middle' };
        mainTitle.height = 35;
        worksheet.mergeCells('A4:G4');

        const subMainTitle = worksheet.addRow(['( Có giá trị như giấy đề ghị bán hàng, giấy đề nghị xuất hóa đơn, lệnh giao hàng)']);
        subMainTitle.font = { name: 'Times New Roman', size: 11, italic: true };
        subMainTitle.alignment = { horizontal: 'center', vertical: 'middle' };
        worksheet.mergeCells('A5:G5');

        const khachHang = worksheet.addRow(['', '', '', '', '', '', '']);
        khachHang.getCell(1).value = {
            richText: [
                { font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: 'Khách hàng: ' },
                { font: { name: 'Times New Roman', size: 11 }, text: odStore.order.cardName }
            ]
        };
        worksheet.mergeCells('A6:E6');
        khachHang.getCell(6).value = {
            richText: [
                { font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: 'Đơn hàng số: ' },
                { font: { name: 'Times New Roman', size: 11 }, text: odStore.order.invoiceCode }
            ]
        };
        worksheet.mergeCells('F6:G6');

        const diaChi = worksheet.addRow(['', '', '', '', '', '', '']);
        diaChi.getCell(1).value = {
            richText: [
                { font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: 'Địa chỉ: ' },
                { font: { name: 'Times New Roman', size: 11 }, text: odStore.order?.address ? `${odStore.order?.address[0].address || ''}, ${odStore.order?.address[0].locationName || ''}, ${odStore.order?.address[0].areaName || ''}` : '' }
            ]
        };
        worksheet.mergeCells('A7:E7');

        diaChi.getCell(6).value = {
            richText: [
                { font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: 'Ngày đặt hàng: ' },
                { font: { name: 'Times New Roman', size: 11 }, text: `${format(odStore.order?.docDate, 'HH:mm - dd/MM/yyyy')}` }
            ]
        };
        worksheet.mergeCells('G7:F7');

        const mst = worksheet.addRow(['', '', '', '', '', '', '']);

        mst.getCell(1).value = {
            richText: [
                { font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: 'MST: ' },
                { font: { name: 'Times New Roman', size: 11 }, text: `${odStore.customer?.licTradNum}` }
            ]
        };
        worksheet.mergeCells('A8:E8');

        mst.getCell(6).value = {
            richText: [
                { font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: 'Ngày giao hàng: ' },
                { font: { name: 'Times New Roman', size: 11 }, text: `${format(odStore.order?.deliveryTime, 'HH:mm - dd/MM/yyyy')}` }
            ]
        };
        // Thêm border cho các ô cụ thể
        const borderCells = [
            { cell: 'A6', borders: ['top', 'left', 'bottom', 'right'] },
            { cell: 'F6', borders: ['top', 'bottom', 'right'] },
            { cell: 'A7', borders: ['top', 'left', 'bottom', 'right'] },
            { cell: 'F7', borders: ['top', 'bottom', 'right'] },
            { cell: 'A8', borders: ['top', 'left', 'bottom', 'right'] },
            { cell: 'F8', borders: ['top', 'bottom', 'right'] }
        ];

        borderCells.forEach(({ cell, borders }) => {
            const borderStyle = { style: 'medium', color: { argb: '000000' } };
            const cellBorder: any = {};
            borders.forEach((side) => {
                cellBorder[side] = borderStyle;
            });
            worksheet.getCell(cell).border = cellBorder;
        });
        worksheet.mergeCells('G8:F8');
        // Định dạng cột
        worksheet.getColumn('A').width = 15;
        worksheet.getColumn('B').width = 15;
        worksheet.getColumn('C').width = 15;
        worksheet.getColumn('D').width = 15;
        worksheet.getColumn('E').width = 15;
        worksheet.getColumn('F').width = 15;
        worksheet.getColumn('G').width = 20;
        worksheet.getColumn('F').numFmt = '""#,##0';
        worksheet.getColumn('G').numFmt = '""#,##0';

        // Tiêu đề cột
        const headerRow = worksheet.addRow(['STT', 'Tên sản phẩm', 'Mã hàng', 'ĐVT', 'Số lượng', 'Đơn giá', 'Thành tiền']);

        // Set font cho từng ô trong header
        for (let col = 1; col <= 7; col++) {
            headerRow.getCell(col).font = { name: 'Times New Roman', size: 11, bold: true };
            headerRow.getCell(col).alignment = { horizontal: 'center', vertical: 'middle' };
        }
        worksheet.getRow(headerRow.number).height = 20;

        // Định dạng màu nền và viền
        const ranges = ['A9', 'B9', 'C9', 'D9', 'E9', 'F9', 'G9'];
        ranges.forEach((range) => {
            worksheet.getCell(range).fill = {
                type: 'pattern',
                pattern: 'solid',
                fgColor: { argb: 'c6e1b5' }
            };
            worksheet.getCell(range).border = {
                top: { style: 'medium', color: { argb: '000000' } },
                left: { style: 'medium', color: { argb: '000000' } },
                bottom: { style: 'medium', color: { argb: '000000' } },
                right: { style: 'medium', color: { argb: '000000' } }
            };
        });

        // Thêm dữ liệu
        _.forEach(odStore.order?.itemDetail, (item, index) => {
            const dataRow = worksheet.addRow([
                index + 1, // STT
                (item as any).itemName || '', // Tên sản phẩm
                (item as any).itemCode || '', // Mã hàng
                (item as any).uomName || (item as any).unitName || '', // ĐVT
                (item as any).quantity || 0, // Số lượng
                (item as any).priceAfterDist || 0, // Đơn giá
                ((item as any).quantity || 0) * ((item as any).priceAfterDist || 0) // Thành tiền
            ]);

            // Định dạng font và alignment cho dữ liệu từng ô
            for (let col = 1; col <= 7; col++) {
                dataRow.getCell(col).font = { name: 'Times New Roman', size: 11 };
                dataRow.getCell(col).alignment = { vertical: 'middle' };
            }

            // Thêm border cho tất cả các ô trong hàng
            for (let col = 1; col <= 7; col++) {
                worksheet.getCell(dataRow.number, col).border = {
                    top: { style: 'medium', color: { argb: '000000' } },
                    left: { style: 'medium', color: { argb: '000000' } },
                    bottom: { style: 'medium', color: { argb: '000000' } },
                    right: { style: 'medium', color: { argb: '000000' } }
                };
            }
        });

        const lastRowIndex = worksheet.rowCount;

        const tienHangTCK = worksheet.insertRow(lastRowIndex + 1, ['', '', '', '', '', '', '']);
        tienHangTCK.getCell(1).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true }, text: 'Tiền hàng trước chiết khấu: ' }]
        };
        tienHangTCK.getCell(7).value = Number(odStore.order?.paymentInfo.totalBeforeVat) || 0;
        tienHangTCK.getCell(7).font = { name: 'Times New Roman', size: 11 };
        tienHangTCK.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
        tienHangTCK.getCell(7).alignment = { vertical: 'middle' };
        worksheet.mergeCells(`A${lastRowIndex + 1}:F${lastRowIndex + 1}`);

        const chietKhauDH = worksheet.insertRow(lastRowIndex + 2, ['', '', '', '', '', '', '']);
        chietKhauDH.getCell(1).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true }, text: 'Chiết khấu đơn hàng: ' }]
        };
        chietKhauDH.getCell(7).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11 }, text: `-` }]
        };
        chietKhauDH.getCell(7).value = Number(odStore.order?.paymentInfo.bonusCommited || 0) || 0;
        chietKhauDH.getCell(7).font = { name: 'Times New Roman', size: 11 };
        chietKhauDH.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
        chietKhauDH.getCell(7).alignment = { vertical: 'middle' };
        worksheet.mergeCells(`A${lastRowIndex + 2}:F${lastRowIndex + 2}`);

        const tienHangSauCK = worksheet.insertRow(lastRowIndex + 3, ['', '', '', '', '', '', '']);
        tienHangSauCK.getCell(1).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true }, text: 'Tiền hàng sau chiết khấu: ' }]
        };

        tienHangSauCK.getCell(7).value = Number(odStore.order?.paymentInfo.totalBeforeVat - odStore.order?.paymentInfo.bonusCommited) || 0;
        tienHangSauCK.getCell(7).font = { name: 'Times New Roman', size: 11 };

        tienHangSauCK.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
        tienHangSauCK.getCell(7).alignment = { vertical: 'middle' };
        worksheet.mergeCells(`A${lastRowIndex + 3}:F${lastRowIndex + 3}`);

        const VAT = worksheet.insertRow(lastRowIndex + 4, ['', '', '', '', '', '', '']);
        VAT.getCell(1).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true }, text: 'VAT ' + addressDefault.vat + ': ' }]
        };
        VAT.getCell(7).value = Number(odStore.order?.paymentInfo.vatAmount || 0);
        VAT.getCell(7).font = { name: 'Times New Roman', size: 11 };
        VAT.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
        VAT.getCell(7).alignment = { vertical: 'middle' };
        worksheet.mergeCells(`A${lastRowIndex + 4}:F${lastRowIndex + 4}`);

        const tongCong = worksheet.insertRow(lastRowIndex + 5, ['', '', '', '', '', '', '']);
        tongCong.getCell(1).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true }, text: 'Tổng hóa đơn: ' }]
        };

        tongCong.getCell(7).value = Number(odStore.order?.paymentInfo?.totalAfterVat);
        tongCong.getCell(7).font = { name: 'Times New Roman', size: 11 };

        tongCong.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
        tongCong.getCell(7).alignment = { vertical: 'middle' };
        worksheet.mergeCells(`A${lastRowIndex + 5}:F${lastRowIndex + 5}`);

        const thuongTT = worksheet.insertRow(lastRowIndex + 6, ['', '', '', '', '', '', '']);
        thuongTT.getCell(1).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true }, text: 'Thưởng thanh toán: ' }]
        };
        thuongTT.getCell(7).value = Number(odStore.order?.paymentInfo?.bonusAmount);
        thuongTT.getCell(7).font = { name: 'Times New Roman', size: 11 };

        thuongTT.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
        thuongTT.getCell(7).alignment = { vertical: 'middle' };
        worksheet.mergeCells(`A${lastRowIndex + 6}:F${lastRowIndex + 6}`);

        const tongTT = worksheet.insertRow(lastRowIndex + 7, ['', '', '', '', '', '', '']);
        tongTT.getCell(1).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true }, text: 'Tổng thanh toán: ' }]
        };
        tongTT.getCell(7).value = Number(odStore.order?.paymentInfo?.totalAfterVat || 0) - (odStore.order?.paymentInfo?.bonusAmount || 0);
        tongTT.getCell(7).font = { name: 'Times New Roman', size: 11 };

        tongTT.getCell(1).alignment = { horizontal: 'center', vertical: 'middle' };
        tongTT.getCell(7).alignment = { vertical: 'middle' };
        worksheet.mergeCells(`A${lastRowIndex + 7}:F${lastRowIndex + 7}`);

        // Thêm border cho các dòng tổng kết
        const summaryRows = [lastRowIndex + 1, lastRowIndex + 2, lastRowIndex + 3, lastRowIndex + 4, lastRowIndex + 5, lastRowIndex + 6, lastRowIndex + 7];
        summaryRows.forEach((rowIndex) => {
            for (let col = 1; col <= 7; col++) {
                worksheet.getCell(rowIndex, col).border = {
                    top: { style: 'medium', color: { argb: '000000' } },
                    left: { style: 'medium', color: { argb: '000000' } },
                    bottom: { style: 'medium', color: { argb: '000000' } },
                    right: { style: 'medium', color: { argb: '000000' } }
                };
            }
        });

        const lastRowIndexFromTongCong = worksheet.rowCount;

        const SPKM = worksheet.insertRow(lastRowIndexFromTongCong + 1, ['', '', '', '', '', '', '']);
        SPKM.getCell(1).value = {
            richText: [
                { font: { name: 'Times New Roman', size: 11, bold: true }, text: 'Sản Phẩm Khuyến Mãi + Vật Phẩm Khuyến Mãi ( Nếu có ) ' },
                { font: { name: 'Times New Roman', size: 11 }, text: `` }
            ]
        };
        SPKM.height = 30;
        SPKM.alignment = { vertical: 'middle' };
        worksheet.mergeCells(`A${lastRowIndexFromTongCong + 1}:G${lastRowIndexFromTongCong + 1}`);

        const headerRowSPKM = worksheet.insertRow(lastRowIndexFromTongCong + 2, ['STT', 'Tên sản phẩm', 'Mã hàng', 'ĐVT', 'Số lượng', 'Đơn giá', 'Thành tiền']);

        // Set font cho từng ô trong header
        for (let col = 1; col <= 7; col++) {
            headerRowSPKM.getCell(col).font = { name: 'Times New Roman', size: 11, bold: true };
            headerRowSPKM.getCell(col).alignment = { horizontal: 'center', vertical: 'middle' };
        }
        worksheet.getRow(headerRowSPKM.number).height = 20;

        // Định dạng màu nền và viền
        const ranges2 = [
            `A${lastRowIndexFromTongCong + 2}`,
            `B${lastRowIndexFromTongCong + 2}`,
            `C${lastRowIndexFromTongCong + 2}`,
            `D${lastRowIndexFromTongCong + 2}`,
            `E${lastRowIndexFromTongCong + 2}`,
            `F${lastRowIndexFromTongCong + 2}`,
            `G${lastRowIndexFromTongCong + 2}`
        ];
        ranges2.forEach((range) => {
            worksheet.getCell(range).fill = {
                type: 'pattern',
                pattern: 'solid',
                fgColor: { argb: 'c6e1b5' }
            };
            worksheet.getCell(range).border = {
                top: { style: 'medium', color: { argb: '000000' } },
                left: { style: 'medium', color: { argb: '000000' } },
                bottom: { style: 'medium', color: { argb: '000000' } },
                right: { style: 'medium', color: { argb: '000000' } }
            };
        });
        _.forEach(
            odStore.order.promotion.filter((el) => el.quantityAdd),
            (el, index) => {
                const dataRow = worksheet.addRow([index + 1, el.itemName, el.itemCode, el.packingName, el.quantityAdd, '-', '-']);
                for (let col = 1; col <= 7; col++) {
                    dataRow.getCell(col).font = { name: 'Times New Roman', size: 11 };
                    dataRow.getCell(col).alignment = { horizontal: 'center', vertical: 'middle' };
                }

                for (let col = 1; col <= 7; col++) {
                    worksheet.getCell(dataRow.number, col).border = {
                        top: { style: 'medium', color: { argb: '000000' } },
                        left: { style: 'medium', color: { argb: '000000' } },
                        bottom: { style: 'medium', color: { argb: '000000' } },
                        right: { style: 'medium', color: { argb: '000000' } }
                    };
                }
            }
        );
        const lastRowFromSPKM = worksheet.rowCount;
        const tongSoLit = worksheet.insertRow(lastRowFromSPKM + 1, ['', '', '', '', '', '', '']);
        tongSoLit.getCell(1).value = {
            richText: [
                { font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: '* Tổng số lít: ' },
                { font: { name: 'Times New Roman', size: 11 }, text: `${odStore.order?.itemDetail.reduce((sum, item) => item.itemVolume * item.quantity + sum, 0)} (lít)` }
            ]
        };

        worksheet.mergeCells(`A${lastRowFromSPKM + 1}:G${lastRowFromSPKM + 1}`);

        const soXeNhanHang = worksheet.insertRow(lastRowFromSPKM + 2, ['', '', '', '', '', '', '']);
        soXeNhanHang.getCell(1).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: `* Số xe nhận hàng: ${shipAddress.value?.vehiclePlate || '-'}` }]
        };
        soXeNhanHang.getCell(4).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: `* Tên người nhận hàng: ${shipAddress.value?.person || '-'}` }]
        };
        worksheet.mergeCells(`A${lastRowFromSPKM + 2}:C${lastRowFromSPKM + 2}`);
        worksheet.mergeCells(`D${lastRowFromSPKM + 2}:G${lastRowFromSPKM + 2}`);

        const HTTT = worksheet.insertRow(lastRowFromSPKM + 3, ['', '', '', '', '', '', '']);
        HTTT.getCell(1).value = {
            richText: [
                {
                    font: { name: 'Times New Roman', size: 11, bold: true, italic: true },
                    text: `* Hình thức thanh toán :  ${odStore.order?.payments ? formatPaymentType(odStore.order?.payments?.find((el) => el.paymentMethodCode === 'PayNow' && el.status === 'A')?.paymentMethodName) : '-'}`
                }
            ]
        };
        HTTT.getCell(4).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: '* Thời gian thanh toán :' }]
        };

        worksheet.mergeCells(`A${lastRowFromSPKM + 3}:C${lastRowFromSPKM + 3}`);
        worksheet.mergeCells(`D${lastRowFromSPKM + 3}:G${lastRowFromSPKM + 3}`);

        const ghiChu = worksheet.insertRow(lastRowFromSPKM + 4, ['', '', '', '', '', '', '']);
        ghiChu.getCell(1).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: '* Ghi chú :  ' }]
        };

        const comfirmBy = worksheet.insertRow(lastRowFromSPKM + 6, ['', '', '', '', '', '', '']);
        comfirmBy.getCell(5).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: `Xác nhận bởi P. DVCSKH  ` }]
        };
        worksheet.mergeCells(`E${lastRowFromSPKM + 6}:G${lastRowFromSPKM + 6}`);
        const comfirmAt = worksheet.insertRow(lastRowFromSPKM + 7, ['', '', '', '', '', '', '']);
        comfirmAt.getCell(5).value = {
            richText: [{ font: { name: 'Times New Roman', size: 11, bold: true, italic: true }, text: `Thời gian xác nhận :  ${odStore?.order?.confirmAt ? format(odStore?.order?.confirmAt, 'HH:mm - dd/MM/yyyy') : ''} ` }]
        };
        worksheet.mergeCells(`E${lastRowFromSPKM + 7}:G${lastRowFromSPKM + 7}`);
        // Xuất file
        const now = new Date();
        const fileName = `Đơn_hàng_${now.getDate()}-${now.getMonth() + 1}-${now.getFullYear()}_${now.getHours()}-${now.getMinutes()}.xlsx`;
        const buffer = await workbook.xlsx.writeBuffer();
        const blob = new Blob([buffer], { type: 'application/octet-stream' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = fileName;
        link.click();
    } finally {
    }
};
const formatPaymentType = (status: any) => {
    switch (status) {
        case 'Bank':
            return 'UNC/Chuyển khoản ngân hàng';
        case 'OnePay':
            return 'OnePay';
        default:
            return '';
    }
};
const shipAddress = computed(() => {
    const adrs = odStore.order?.address.find((x) => x.type == 'S');
    const address = {} as typeof adrs & { fullAddress: string };
    if (adrs) {
        Object.assign(address, adrs);
    }
    address.fullAddress = [address.address, address.locationName, address.areaName].filter((x) => x).join(', ');
    return address;
});
</script>

<style scoped>
.format-option {
    transition: all 0.2s;
    cursor: pointer;
    flex: 1;
}

.format-option:hover {
    background-color: var(--surface-100);
}

.format-option.selected {
    background-color: var(--surface-200);
    border: 1px solid var(--primary-color);
}

label {
    cursor: pointer;
}

label:hover {
    background: transparent;
}
</style>
