// ─── Import thư viện ngoài ───────────────────────────────────────────────────
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { ToastServiceMethods } from 'primevue/toastservice';

// ─── Import nội bộ ──────────────────────────────────────────────────────────
import API from '@/api/api-main';
import { useHierarchyStore } from '@/Pinia/hierarchyStore';
import { Validator, ValidateResult, ValidateOption } from '@/helpers/validate';

// ─── Hằng số ────────────────────────────────────────────────────────────────
const QUARTER_LABELS = ['I', 'II', 'III', 'IV'] as const;

const LINE_STATUS = {
    ACTIVE: 'A',
    UPDATED: 'U',
    DELETED: 'D',
    PENDING: 'P',
} as const;

// ─── Danh sách ngành hàng dùng chung (lazy singleton) ───────────────────────
const industryOptions = ref<Array<any>>([]);
let _industryLoaded = false;

/**
 * Tải danh sách ngành hàng từ API (chỉ gọi 1 lần duy nhất).
 * Sử dụng pattern lazy singleton để tránh gọi API nhiều lần.
 */
function loadIndustryOptions(cardId?: number | null) {
    if (_industryLoaded) return;
    _industryLoaded = true;
    const url = cardId ? `Industry/getallnew?cardId=${cardId}` : 'Industry/getallnew';
    API.get(url).then((res) => {
        industryOptions.value = res.data;
    });
}

/**
 * Buộc tải lại danh sách ngành hàng (dùng khi mở màn hình chi tiết hoặc chọn khách hàng).
 */
export function refreshIndustryOptions(cardId?: number | null) {
    _industryLoaded = true;
    const url = cardId ? `Industry/getallnew?cardId=${cardId}` : 'Industry/getallnew';
    API.get(url).then((res) => {
        industryOptions.value = res.data;
    });
}

// ─── Định nghĩa kiểu dữ liệu ────────────────────────────────────────────────
export type DocStatus = 'P' | 'C' | 'D' | 'A' | 'R';
export type CommittedType = 'Y' | 'Q' | 'P';
type PropOption = 'industryId' | 'brandIds' | 'itemTypeIds';

// ─── Giao diện (Interfaces) ─────────────────────────────────────────────────
export interface CommittedResponse {
    data: Array<Committed>;
    total: number;
    skip: number;
    limit: number;
}

export interface Creator {
    isSupperUser: boolean;
    roleId: number;
    personRoleId: number;
    personRole: any;
    role: any;
    userRoles: any;
    fullName: string;
    phone: string;
    userType: string;
    note: string;
    dob: string;
    status: string;
    cardId: any;
    bpInfo: any;
    userGroup: any;
    lastLogin: string;
    organizationId: number;
    directStaff: [];
    id: number;
    userName: string;
    normalizedUserName: string;
    email: string;
    normalizedEmail: string;
    phonenumber: any;
}

export interface Industry {
    id: number;
    code: string;
    name: string;
    brands: Brand[];
    sapCode: string;
}

export interface Brand {
    id: number;
    code: string;
    name: string;
    sapCode: string;
}

export interface CommittedLineSubSub {
    id: number;
    outPut: number;
    discount: number;
    isConvert: boolean;
    status: string;
}

export interface CommittedLineSub {
    id: number;
    fatherId: number;
    industryId: number;
    brandIds: number[];
    industryName: string;
    brandId: null;
    itemTypeIds: number[];
    itemTypes: any[];
    quarter1: number | null;
    quarter2: number | null;
    quarter3: number | null;
    quarter4: number | null;
    package: number | null;
    month1: number | null;
    month2: number | null;
    month3: number | null;
    month4: number | null;
    month5: number | null;
    month6: number | null;
    month7: number | null;
    month8: number | null;
    month9: number | null;
    month10: number | null;
    month11: number | null;
    month12: number | null;
    total: number | null;
    discount: number;
    discountYear: number;
    isCvYear: boolean;
    isConvert: boolean;
    discountMonth: number | null;
    isCvMonth: boolean;
    threeMonthDiscount: number | null;
    isCvThreeMonth: boolean;
    sixMonthDiscount: number | null;
    isCvSixMonth: boolean;
    nineMonthDiscount: number | null;
    isCvNineMonth: boolean;
    currentVolumn: number | null;
    afterVolumn: number | null;
    isAchieved: boolean;
    bonusPercentage: number | null;
    totalBonus: number | null;
    industry: Industry;
    status: string;
    brand: Brand[];
    committedLineSubSub: CommittedLineSubSub[];
}

export interface CommittedLine {
    id: number;
    fatherId: number;
    committedType: CommittedType;
    status: string;
    committedLineSub: CommittedLineSub[];
}

export interface Committed {
    id: number;
    committedCode: string | null;
    committedName: string | null;
    committedDescription: string | null;
    cardId: number | null;
    cardCode?: string;
    cardName?: string;
    committedYear: string | Date | null;
    userId: number;
    creator?: Creator;
    docStatus: DocStatus;
    rejectReason: string;
    committedLine: CommittedLine[];
    userType: any;
}

export interface Column {
    field: string;
    header: string;
    componentName: string;
    isPercent?: boolean;
    convertField?: string;
    disabled?: boolean;
    class?: string;
    value?: (row: any, type: CommittedType) => number;
    optionLabel?: string;
    optionValue?: string;
    onChange?: Function;
}

export interface SaveStatus {
    success: boolean;
    linesError: Record<string, any> | null;
    errors: any;
}

// ─── Cấu hình kiểm tra dữ liệu ─────────────────────────────────────────────
const validateOption: ValidateOption = {
    committedName: {
        validators: {
            type: String,
            required: true,
            nullMessage: 'Nhập tên cam kết',
        },
    },
    cardId: {
        validators: {
            required: true,
            type: Number,
            nullMessage: 'Chọn đối tượng áp dụng',
        },
    },
    committedYear: {
        validators: {
            required: true,
            type: Date,
            nullMessage: 'Chọn năm cam kết',
        },
    },
};

// ─── Hàm tiện ích ────────────────────────────────────────────────────────────
/**
 * Gán giá trị vào object lồng nhau theo danh sách key (đệ quy).
 */
function setPropMap(target: any, value: any, levels: Array<string | number>, level = 0): any {
    if (!target) target = {};
    if (level === levels.length - 1) {
        target[levels[level]] = value;
        return target;
    }
    if (!target[levels[level]]) target[levels[level]] = {};
    target[levels[level]] = setPropMap(target[levels[level]], value, levels, level + 1);
    return target;
}

/**
 * Tính tổng sản lượng theo loại cam kết (Q = theo quý, Y = theo tháng).
 */
function calcTotal(row: any, type: CommittedType): number {
    if (type === 'Q') {
        return [1, 2, 3, 4].reduce((sum, i) => sum + (row[`quarter${i}`] || 0), 0);
    }
    if (type === 'Y') {
        return Array.from({ length: 12 }, (_, i) => i + 1).reduce(
            (sum, i) => sum + (row[`month${i}`] || 0),
            0,
        );
    }
    return 0;
}

// ─── Các lớp (Classes) ───────────────────────────────────────────────────────
export class CommittedLineSub implements CommittedLineSub {
    committedLineSubSub: CommittedLineSubSub[] = [];

    /**
     * Khởi tạo dòng cam kết chi tiết.
     * @param src - Dữ liệu nguồn từ API (nếu có). Nếu không truyền, khởi tạo dòng mới rỗng.
     */
    constructor(src?: any) {
        if (src) {
            Object.assign(this, src);
            this.itemTypeIds = src.itemTypes.map((item: any) => item.id);
            this.brandIds = src.brand.map((item: any) => item.id);
            this.status = LINE_STATUS.UPDATED;
        } else {
            for (let i = 1; i <= 12; i++) (this as any)[`month${i}`] = null;
            for (let i = 1; i <= 4; i++) (this as any)[`quarter${i}`] = null;
            this.nineMonthDiscount = null;
            this.sixMonthDiscount = null;
            this.threeMonthDiscount = null;
            this.total = 0;
            this.status = LINE_STATUS.ACTIVE;
        }
    }

    /** Lấy danh sách dòng sản lượng vượt còn hiệu lực (bỏ qua các dòng đã xóa). */
    getCommittedLineSubSub(): CommittedLineSubSub[] {
        return this.committedLineSubSub.filter((line) => line.status !== LINE_STATUS.DELETED);
    }

    /**
     * Xử lý sự kiện khi người dùng thay đổi lựa chọn trên một trường.
     * - Đổi ngành hàng → reset thương hiệu và loại sản phẩm.
     * - Đổi thương hiệu → reset loại sản phẩm.
     * @param field - Tên trường vừa được thay đổi.
     */
    onChangeSelect(field: PropOption): void {
        if (field === 'industryId') {
            this.brandIds = [];
            this.itemTypeIds = [];
        } else if (field === 'brandIds') {
            this.itemTypeIds = [];
        }
    }

    /** Thêm một dòng sản lượng vượt mới (mặc định rỗng) vào danh sách. */
    addSubSubLine(): void {
        this.committedLineSubSub.push({
            id: 0,
            discount: 0,
            isConvert: false,
            status: LINE_STATUS.ACTIVE,
        } as CommittedLineSubSub);
    }

    /**
     * Lấy danh sách tùy chọn cho dropdown/multiselect theo từng trường.
     * @param field - Tên trường cần lấy options ('industryId' | 'brandIds' | 'itemTypeIds').
     * @param industryId - ID ngành hàng đang chọn (dùng để lọc thương hiệu và loại sản phẩm).
     * @param brandIds - Danh sách ID thương hiệu đang chọn (dùng để lọc loại sản phẩm).
     * @param cardId - ID khách hàng (dùng để lọc ngành hàng theo khách hàng).
     * @returns Mảng các tùy chọn tương ứng với trường được yêu cầu.
     */
    getSelectionOptions(
        field: PropOption,
        industryId?: number,
        brandIds?: number[],
        cardId?: number | null,
    ): Array<any> {
        loadIndustryOptions(cardId);
        const hierarchyStore = useHierarchyStore();

        switch (field) {
            case 'industryId':
                return industryOptions.value.map(({ brands: _b, ...row }) => row);

            case 'brandIds':
                return industryOptions.value.find((ind) => ind.id === industryId)?.brands ?? [];

            case 'itemTypeIds':
                hierarchyStore.loadHierarchies(cardId);
                return hierarchyStore.getItemTypeOptions(brandIds ?? [], industryId ? [industryId] : []);

            default:
                return [];
        }
    }

    /**
     * Xóa một dòng sản lượng vượt theo chỉ số hiển thị.
     * - Nếu dòng đã lưu (có id) → đánh dấu xóa mềm (status = DELETED).
     * - Nếu dòng mới tạo (chưa có id) → xóa cứng khỏi mảng.
     * @param index - Chỉ số trong danh sách hiển thị (không phải mảng gốc).
     */
    removeSubSubLine(index: number): void {
        const visible = this.getCommittedLineSubSub();
        const target = visible[index];
        if (target.id) {
            target.status = LINE_STATUS.DELETED;
        } else {
            const xIndex = this.committedLineSubSub.indexOf(target);
            this.committedLineSubSub.splice(xIndex, 1);
        }
    }
}

export class CommittedLine implements CommittedLine {
    status: string = LINE_STATUS.ACTIVE;
    committedType: CommittedType = 'Q';
    committedLineSub: CommittedLineSub[] = [];

    /**
     * Khởi tạo dòng cam kết (CommittedLine).
     * @param src - Dữ liệu nguồn từ API (nếu có). Nếu không truyền, tạo dòng mới với một dòng chi tiết rỗng.
     */
    constructor(src?: any) {
        if (src) {
            Object.assign(this, src);
            this.status = LINE_STATUS.UPDATED;
            this.committedLineSub = src.committedLineSub.map((sub: any) => new CommittedLineSub(sub));
        } else {
            this.addSubLine();
            this.status = LINE_STATUS.ACTIVE;
        }
    }

    /**
     * Lấy danh sách cột hiển thị cho bảng nhập liệu cam kết.
     * Cấu trúc cột thay đổi tùy theo loại cam kết:
     * - 'Q' (Quý): hiển thị 4 cột quý + chiết khấu quý/6 tháng/9 tháng.
     * - 'Y' (Năm): hiển thị 12 cột tháng + chiết khấu tháng/3 tháng/6 tháng/9 tháng.
     * @param type - Loại cam kết ('Q' = Quý, 'Y' = Năm).
     * @returns Mảng cấu hình cột cho DataTable.
     */
    getColumns(type: CommittedType): Column[] {
        const { t } = useI18n();

        const totalCol: Column = {
            field: 'total',
            header: t('body.sampleRequest.commitment.table_header_total_volume'),
            componentName: 'InputNumber',
            class: 'text-right',
            isPercent: false,
            disabled: true,
            value: calcTotal,
        };

        const baseCols: Column[] = [
            { field: 'industryId', header: t('client.industry'), componentName: 'Dropdown', optionLabel: 'name', optionValue: 'id' },
            { field: 'brandIds', header: t('client.brand'), componentName: 'MultiSelect', optionLabel: 'name', optionValue: 'id' },
            { field: 'itemTypeIds', header: t('client.product_type'), componentName: 'MultiSelect', optionLabel: 'itemTypeName', optionValue: 'itemTypeId', class: 'w-20rem' },
        ];

        const discountYearCol: Column = {
            field: 'discountYear',
            header: t('body.sampleRequest.commitment.discount_yearly'),
            componentName: 'InputNumber',
            class: 'text-right',
            isPercent: true,
            convertField: 'isCvYear',
        };

        if (type === 'Q') {
            const quarterCols: Column[] = QUARTER_LABELS.map((label, idx) => ({
                field: `quarter${idx + 1}`,
                header: `${t('client.quarter')} ${label} ${t('client.liter')}`,
                componentName: 'InputNumber',
                class: 'text-right',
            }));
            return [
                ...baseCols,
                ...quarterCols,
                totalCol,
                { field: 'discount', header: t('body.sampleRequest.commitment.discount_quarterly'), componentName: 'InputNumber', class: 'text-right', isPercent: true, convertField: 'isConvert' },
                { field: 'sixMonthDiscount', header: t('body.sampleRequest.commitment.discount_6_months'), componentName: 'InputNumber', class: 'text-right', isPercent: true, convertField: 'isCvSixMonth' },
                { field: 'nineMonthDiscount', header: t('body.sampleRequest.commitment.discount_9_months'), componentName: 'InputNumber', class: 'text-right', isPercent: true, convertField: 'isCvNineMonth' },
                discountYearCol,
            ];
        }

        if (type === 'Y') {
            const monthCols: Column[] = Array.from({ length: 12 }, (_, i) => ({
                field: `month${i + 1}`,
                header: `Tháng ${i + 1} (Lít)`,
                componentName: 'InputNumber',
                class: 'text-right',
            }));
            return [
                ...baseCols,
                ...monthCols,
                totalCol,
                { field: 'discountMonth', header: t('body.sampleRequest.commitment.discount_month'), componentName: 'InputNumber', class: 'text-right', isPercent: true, convertField: 'isCvMonth' },
                { field: 'threeMonthDiscount', header: t('body.sampleRequest.commitment.discount_3_months'), componentName: 'InputNumber', class: 'text-right', isPercent: true, convertField: 'isCvThreeMonth' },
                { field: 'sixMonthDiscount', header: t('body.sampleRequest.commitment.discount_6_months'), componentName: 'InputNumber', class: 'text-right', isPercent: true, convertField: 'isCvSixMonth' },
                { field: 'nineMonthDiscount', header: t('body.sampleRequest.commitment.discount_9_months'), componentName: 'InputNumber', class: 'text-right', isPercent: true, convertField: 'isCvNineMonth' },
                discountYearCol,
            ];
        }

        return [...baseCols, discountYearCol];
    }

    /** Thêm một dòng chi tiết cam kết mới (rỗng) vào cuối danh sách. */
    addSubLine(): void {
        this.committedLineSub.push(new CommittedLineSub());
    }

    /** Lấy danh sách dòng chi tiết cam kết còn hiệu lực (bỏ qua các dòng đã xóa). */
    getCommittedLineSub(): CommittedLineSub[] {
        return this.committedLineSub.filter((sub) => sub.status !== LINE_STATUS.DELETED);
    }

    /**
     * Xóa một dòng chi tiết cam kết theo chỉ số hiển thị.
     * - Nếu dòng đã lưu (có id) → đánh dấu xóa mềm (status = DELETED).
     * - Nếu dòng mới tạo (chưa có id) → xóa cứng khỏi mảng.
     * @param index - Chỉ số trong danh sách hiển thị (không phải mảng gốc).
     */
    removeSubLine(index: number): void {
        const visible = this.getCommittedLineSub();
        const target = visible[index];
        if (target.id) {
            target.status = LINE_STATUS.DELETED;
        } else {
            const xIndex = this.committedLineSub.indexOf(target);
            this.committedLineSub.splice(xIndex, 1);
        }
    }
}

export class Committed implements Committed {
    committedLine: CommittedLine[] = [];

    /**
     * Khởi tạo đối tượng cam kết.
     * @param src - Dữ liệu nguồn từ API (nếu có). Nếu không truyền, tạo phiếu cam kết mới rỗng.
     */
    constructor(src?: any) {
        if (src) {
            delete src.creator; // Bỏ thông tin người tạo để tránh gửi thừa dữ liệu
            Object.assign(this, src);
            this.committedYear = this.committedYear ? new Date(this.committedYear) : null;
            this.committedLine = src.committedLine.map((line: any) => new CommittedLine(line));
        } else {
            this.addCommittedLine();
            this.committedCode = '';
            this.committedName = '';
            this.committedDescription = null;
            this.committedYear = null;
            this.cardId = null;
        }
    }

    /**
     * Lưu hoặc gửi duyệt phiếu cam kết.
     * Thực hiện kiểm tra dữ liệu trước khi gọi API:
     * - Validate các trường header (tên, đối tượng, năm cam kết).
     * - Kiểm tra từng dòng cam kết phải có đủ ngành hàng, thương hiệu, loại sản phẩm.
     * @param docStatus - Trạng thái tài liệu cần lưu ('A' = Lưu nháp, 'P' = Gửi duyệt).
     * @param toast - Service hiển thị thông báo cho người dùng.
     * @returns Kết quả lưu bao gồm trạng thái thành công, lỗi từng dòng và lỗi chung.
     */
    async save(docStatus: DocStatus, toast: ToastServiceMethods): Promise<SaveStatus> {
        this.docStatus = docStatus;
        const vresult = Validator(this, validateOption);
        const linesError: Record<string, any> = {};
        let errorCount = 0;
        let errorSLVCount = 0;

        this.committedLine.forEach((line, i) => {
            line.getCommittedLineSub().forEach((lineSub, j) => {
                if (!lineSub.industryId) { setPropMap(linesError, true, [i, j, 'industryId']); errorCount++; }
                if (!lineSub.brandIds?.length) { setPropMap(linesError, true, [i, j, 'brandIds']); errorCount++; }
                if (!lineSub.itemTypeIds?.length) { setPropMap(linesError, true, [i, j, 'itemTypeIds']); errorCount++; }

                lineSub.getCommittedLineSubSub().forEach((lineSubSub, h) => {
                    if (!lineSubSub.outPut) { setPropMap(linesError, true, [i, j, h, 'outPut']); errorSLVCount++; }
                    if (lineSubSub.discount == null) { setPropMap(linesError, true, [i, j, h, 'discount']); errorSLVCount++; }
                });
            });
        });

        if (errorCount > 0 || errorSLVCount > 0 || !vresult.result) {
            if (errorCount > 0) {
                toast.add({ severity: 'warn', summary: 'Lỗi', detail: 'Vui lòng điền đầy đủ thông tin cho các dòng cam kết', life: 3000 });
            } else if (errorSLVCount > 0) {
                toast.add({ severity: 'warn', summary: 'Lỗi', detail: 'Điền thông tin sản lượng vượt', life: 3000 });
            }
            return { success: false, linesError, errors: vresult.errors };
        }

        try {
            const endpoint = this.id ? `Commited/${this.id}` : 'Commited';
            await (this.id ? API.update(endpoint, this) : API.add(endpoint, this));
            toast.add({ severity: 'success', summary: 'Thành công', detail: docStatus === 'P' ? 'Đã gửi cam kết' : 'Lưu thành công', life: 3000 });
            return { success: true, linesError, errors: null };
        } catch (error: any) {
            const customErrors: Record<string, string> = {};
            toast.add({ severity: 'error', summary: 'Lỗi', detail: error?.response?.data?.errors || '500 internal server', life: 3000 });
            return { success: false, linesError, errors: customErrors };
        }
    }

    /** Thêm một dòng cam kết mới (rỗng) vào đầu danh sách. */
    addCommittedLine(): void {
        this.committedLine.unshift(new CommittedLine());
    }

    /** Lấy danh sách dòng cam kết còn hiệu lực (bỏ qua các dòng đã xóa). */
    getCommittedLine(): CommittedLine[] {
        return this.committedLine.filter((line) => line.status !== LINE_STATUS.DELETED);
    }

    /**
     * Xóa một dòng cam kết theo chỉ số hiển thị.
     * - Nếu dòng đã lưu (có id) → đánh dấu xóa mềm (status = DELETED).
     * - Nếu dòng mới tạo (chưa có id) → xóa cứng khỏi mảng.
     * @param index - Chỉ số trong danh sách hiển thị (không phải mảng gốc).
     */
    removeCommittedLine(index: number): void {
        const visible = this.getCommittedLine();
        const target = visible[index];
        if (target.id) {
            target.status = LINE_STATUS.DELETED;
        } else {
            const xIndex = this.committedLine.indexOf(target);
            this.committedLine.splice(xIndex, 1);
        }
    }

    /** Lấy danh sách loại cam kết để hiển thị trong dropdown chọn loại (Quý / Năm). */
    getCmtLineTypeOptions(): { name: string; value: CommittedType }[] {
        return [
            { name: 'Quý', value: 'Q' },
            { name: 'Năm', value: 'Y' },
        ];
    }
}
