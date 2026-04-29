<script setup>
// import thư viện
import { ref, watch, onBeforeMount, onMounted, watchEffect } from "vue";
import { useGlobal } from "@/services/useGlobal";
import API from "@/api/api-main";
import { useRouter, useRoute } from "vue-router";
import { FilterStore } from "@/Pinia/Filter/FilterStoreCommitted";
import { inject } from "vue";
import ExcelJS from "exceljs";
import { debounce } from "lodash";
import ImportExcel from "./components/ImportExcel.vue";

// Biến function

const conditionHandler = inject("conditionHandler");
const router = useRouter();
const route = useRoute();
const { toast, FunctionGlobal } = useGlobal();
// END

// Biến trạng thái
const loadding = ref(false);
const visibleConfirmDlg = ref(false);
const confirmModal = ref(false);
const submited = ref(false);
// END

// Biến lưu trữ data
const expandedRows = ref();
const Hierarchy = ref();
const dataCustomer = ref();
const dataCommited = ref([]);
const dataTable = ref({
    limit: route.query.page ? route.query.page : 10,
    skip: route.query.size ? route.query.size : 0,
});
const dataCommittedType = ref();
const rowSelect = ref();
const committedLine = ref();
const PayloadData = ref();
const Industry = ref();
const IndustryClone = ref();
const filterStore = FilterStore() || {};
const DataStruct = ref({
    structTable: [
        {
            field: "industryId",
            header: " Ngành hàng ",
            require: true,
            typeValue: "Dropdown",
            placeholder: "Chọn ngành hàng",
        },
        {
            field: "brandIds",
            header: "Thương hiệu ",
            require: true,
            typeValue: "MultiSelect",
            placeholder: "Chọn thương hiệu",
        },
        {
            field: "itemTypeIds",
            header: "Loại sản phẩm",
            require: false,
            typeValue: "MultiSelect",
            placeholder: "Chọn sản phẩm",
        },
        {
            field: "quarter1",
            header: " Quý I ",
            typeValue: "InputNumber",
            placeholder: "Nhập quý 1",
            committedType: ["Q"],
        },
        {
            field: "quarter2",
            header: " Quý II ",
            typeValue: "InputNumber",
            placeholder: "Nhập quý 2",
            committedType: ["Q"],
        },
        {
            field: "quarter3",
            header: " Quý III ",
            typeValue: "InputNumber",
            placeholder: "Nhập quý 3",
            committedType: ["Q"],
        },
        {
            field: "quarter4",
            header: " Quý IV ",
            typeValue: "InputNumber",
            placeholder: "Nhập quý 4",
            committedType: ["Q"],
        },
        {
            field: "package",
            header: "Tổng sản lượng",
            typeValue: "InputNumber",
            placeholder: "Nhập tổng sl",
            committedType: ["P"],
            require: true,
        },
        {
            field: "month1",
            header: "Tháng 1",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 1",
            committedType: ["Y"],
        },
        {
            field: "month2",
            header: "Tháng 2",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 2",
            committedType: ["Y"],
        },
        {
            field: "month3",
            header: "Tháng 3",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 3",
            committedType: ["Y"],
        },
        {
            field: "month4",
            header: "Tháng 4",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 4",
            committedType: ["Y"],
        },
        {
            field: "month5",
            header: "Tháng 5",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 5",
            committedType: ["Y"],
        },
        {
            field: "month6",
            header: "Tháng 6",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 6",
            committedType: ["Y"],
        },
        {
            field: "month7",
            header: "Tháng 7",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 7",
            committedType: ["Y"],
        },
        {
            field: "month8",
            header: "Tháng 8",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 8",
            committedType: ["Y"],
        },
        {
            field: "month9",
            header: "Tháng 9",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 9",
            committedType: ["Y"],
        },
        {
            field: "month10",
            header: "Tháng 10",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 10",
            committedType: ["Y"],
        },
        {
            field: "month11",
            header: "Tháng 11",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 11",
            committedType: ["Y"],
        },
        {
            field: "month12",
            header: "Tháng 12",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 12",
            committedType: ["Y"],
        },
        {
            field: "total",
            header: " TC sản lượng ",
            typeValue: "InputNumber",
            disabled: true,
            require: true,
            placeholder: "Nhập tổng sl",
            committedType: ["Y", "Q"],
        },
        {
            field: "discountMonth",
            sub_field: "isCvMonth",
            header: "CK tháng",
            typeValue: "Mixed",
            placeholder: "Nhập ck tháng",
            committedType: ["Y"],
        },
        {
            field: "discount",
            sub_field: "isConvert",
            header: "Chiết khấu quý",
            require: true,
            typeValue: "Mixed",
            placeholder: "Nhập ck quý",
            committedType: ["Q"],
        },
        {
            field: "threeMonthDiscount",
            sub_field: "isCvThreeMonth",
            header: "CK 3 tháng",
            typeValue: "Mixed",
            placeholder: "",
            committedType: ["Y"],
        },
        {
            field: "sixMonthDiscount",
            sub_field: "isCvSixMonth",
            header: "CK 6 tháng",
            typeValue: "Mixed",
            placeholder: "",
            committedType: ["Q", "Y"],
        },
        {
            field: "nineMonthDiscount",
            sub_field: "isCvNineMonth",
            header: "CK 9 tháng",
            typeValue: "Mixed",
            placeholder: "",
            committedType: ["Q", "Y"],
        },
        {
            field: "discountYear",
            sub_field: "isCvYear",
            header: "Chiết khấu năm",
            typeValue: "Mixed",
            placeholder: "Nhập ck",
            committedType: ["Q", "Y"],
            require: true,
        },
        {
            field: "discount",
            sub_field: "isConvert",
            header: "Chiết khấu",
            require: true,
            typeValue: "Mixed",
            placeholder: "Nhập ck",
            committedType: ["P"],
        },
    ],
    structTableChill: [
        {
            field: "outPut",
            header: "Sản lượng vượt",
            require: true,
            typeValue: "InputNumber",
            placeholder: "Nhập sản lượng vượt",
        },
        {
            field: "discount",
            sub_field: "isConvert",
            header: "Chiết khấu ",
            require: true,
            typeValue: "Mixed",
            placeholder: "Nhập chiết khấu",
        },
    ],
});
// END

onMounted(() => {
    if (route.query.id) {
        OpenDetail(route.query.id);
    }
});
watch(route, () => {
    if (route.query.id) {
        OpenDetail(route.query.id);
    }
});
// Begin Function
onBeforeMount(async () => {
    dataCommited.value = await GetCommited();
    dataCustomer.value = await GetCustomer();
    hierarchy.loadHierarchies();
});

// Khởi tạo dữ liệu
const InitData = () => {
    confirmModal.value = false;
    visibleConfirmDlg.value = false;
    submited.value = false;
    expandedRows.value = [];

    dataCommittedType.value = [
        {
            code: "Q",
            name: "Quý",
        },
        {
            code: "P",
            name: "Gói",
        },
        {
            code: "Y",
            name: "Năm",
        },
    ];

    PayloadData.value = {
        id: 0,
        committedCode: "",
        committedName: "",
        committedDescription: "",
        cardId: 0,
        committedYear: "",
        docStatus: "",
        committedLine: [
            {
                id: 0,
                committedType: "",
                status: "",
                committedLineSub: [
                    {
                        id: 0,
                        industryId: null,
                        brandIds: null,
                        quarter1: null,
                        quarter2: null,
                        quarter3: null,
                        quarter4: null,
                        package: null,
                        month1: null,
                        month2: null,
                        month3: null,
                        month4: null,
                        month5: null,
                        month6: null,
                        month7: null,
                        month8: null,
                        month9: null,
                        month10: null,
                        month11: null,
                        month12: null,
                        total: null,
                        discount: null,
                        isConvert: false,
                        discountMonth: null,
                        isCvMonth: false,
                        nineMonthDiscount: null,
                        isCvNineMonth: false,
                        discountYear: null,
                        isCvYear: false,
                        status: "",
                        committedLineSubSub: [
                            {
                                id: 0,
                                outPut: null,
                                discount: null,
                                isConvert: false,
                                status: "",
                            },
                        ],
                    },
                ],
            },
        ],
    };

    committedLine.value = JSON.stringify(PayloadData.value.committedLine[0]);
};
InitData();
const openConfirm = async () => {
    InitData();
    confirmModal.value = true;
    PayloadData.value.committedLine[0].committedType = "Q";
    PayloadData.value.committedLine[0].committedLineSub[0].committedLineSubSub = [];
    dataCommittedType.value.find((el) => el.code == "Q").index = 1;
    // expandedRows.value = [PayloadData.value.committedLine[0].committedLineSub[0]];
};

const AddForm = () => {
    if (PayloadData.value.committedLine.length >= 3) return;
    PayloadData.value.committedLine.push(JSON.parse(committedLine.value));
};

const AddRows = (data, SubSub = false) => {
    const dataClone = JSON.parse(committedLine.value).committedLineSub[0];
    const _committedLine = dataClone;
    if (!SubSub) _committedLine.committedLineSubSub = [];
    const dataRow = SubSub ? dataClone.committedLineSubSub[0] : _committedLine;
    dataRow.key = data.length + 1;
    dataRow.status = "A";
    data.push(dataRow);
};

const DeleteRows = (data, sp, index) => {
    if (sp.id) {
        sp.status = "D";
        return;
    }
    data.splice(index, 1);
};

const GetCustomer = async (filterDeactive = true) => {
    try {
        const res = await API.get("Customer?skip=0&limit=10000");
        return res.data.items?.filter((cus) => {
            if (filterDeactive) {
                return cus.status !== "D";
            } else return true;
        });
    } catch (error) {
        return [];
    }
};

const ChangeTypeCommit = (event, index) => {
    dataCommittedType.value.forEach((el) => {
        if (el.index == index + 1) delete el.index;
    });
    dataCommittedType.value.find((el) => el.code == event.value).index = index + 1;
};

const GetCommited = async (filters = "") => {
    try {
        loadding.value = true;
        const queryParams = `?skip=${dataTable.value.skip}&limit=${dataTable.value.limit}${filters}`;
        const res = await API.get("Commited" + queryParams);
        Object.assign(dataTable.value, {
            limit: res.data.limit,
            skip: res.data.skip,
            total: res.data.total,
        });
        router.push(queryParams);
        return res.data.data;
    } catch (error) {
        return [];
    } finally {
        loadding.value = false;
    }
};

const saveCommited = async (type) => {
    submited.value = true;
    if (validateData()) return;
    if (validateDataLine(PayloadData.value.committedLine.filter((e) => e.status != "D")))
        return;
    try {
        loadding.value = true;
        const total = PayloadData.value.committedLine.find((e) => e.committedType == "P");
        if (total != undefined) {
            total.committedLineSub.forEach((e) => {
                if (e.package) e.total = e.package;
            });
        }
        PayloadData.value.docStatus = type;
        delete PayloadData.value.creator;
        const ENDPOINT = PayloadData.value.id
            ? API.update(`Commited/${PayloadData.value.id}`, PayloadData.value)
            : API.add("Commited", PayloadData.value);

        const { data } = await ENDPOINT;
        if (data) {
            const mess = type == "P" ? "Đã gửi thành công" : "Cập nhật thành công";
            FunctionGlobal.$notify("S", mess, toast);
            dataCommited.value = await GetCommited();
            InitData();
        }
    } catch (error) {
        FunctionGlobal.$notify("E", error.response.data.errors, toast);
    } finally {
        loadding.value = false;
    }
};

const RemoveTypeCommit = (item, index) => {
    dataCommittedType.value.forEach((el) => {
        if (el.index == index + 1) delete el.index;
    });
    if (item.id) {
        item.status = "D";
        return;
    }
    PayloadData.value.committedLine.splice(index, 1);
};

const getByIdCommeted = async (id) => {
    try {
        const res = await API.get(`Commited/${id}`);
        return res.data;
    } catch (error) {
        return [];
    }
};

const OpenDetail = async (id) => {
    ResetData();
    await getHierarchy();
    loadding.value = true;
    PayloadData.value = await getByIdCommeted(id);
    PayloadData.value.committedLine?.forEach((el, index) => {
        el.committedLineSub?.forEach((element) => {
            element.brandIds = element.brand?.map((b) => b.id);
            element.itemTypeIds = element.itemTypes?.map((b) => b.id);
        });
        const dataCmtedType = dataCommittedType.value.find(
            (item) => item.code == el.committedType
        );
        if (dataCmtedType) {
            dataCmtedType.index = index + 1;
        }
    });
    PayloadData.value.committedYear = new Date(PayloadData.value.committedYear);
    expandedRows.value = [PayloadData.value?.committedLine[0]?.committedLineSub[0]] || [];
    setAllStatus(PayloadData.value, "U");
    loadding.value = false;
    confirmModal.value = true;
};

const ResetData = () => {
    InitData();
};

const action = ref("");
const openConfirmDlg = (data, type) => {
    rowSelect.value = data;
    action.value = type;
    visibleConfirmDlg.value = true;
};

const onClickConfirm = async (data) => {
    try {
        loadding.value = true;
        let res = null;
        if (action.value == "C") {
            res = await API.update(`Commited/${data.id}/cancel`);
        }
        if (action.value == "D") {
            res = await API.delete(`Commited/${data.id}`);
        }
        if (res.status === 200)
            FunctionGlobal.$notify(
                "S",
                `${action.value == "C" ? "Hủy" : "Xóa"} thành công!`,
                toast
            );
        dataCommited.value = await GetCommited();
        rowSelect.value = null;
    } catch (error) {
        FunctionGlobal.$notify("E", error.response.data.errors, toast);
    } finally {
        loadding.value = false;
        visibleConfirmDlg.value = false;
    }
};

const validateData = () => {
    let status = false;
    if (!PayloadData.value.committedName) {
        FunctionGlobal.$notify("E", "Vui lòng nhập tên cam kết", toast);
        status = true;
    }
    if (!PayloadData.value.cardId) {
        FunctionGlobal.$notify("E", "Vui lòng chọn khách hàng", toast);
        status = true;
    }
    if (!PayloadData.value.committedYear) {
        FunctionGlobal.$notify("E", "Vui lòng nhập thời gian cam kết", toast);
        status = true;
    }

    return status;
};

const validateDataLine = (data) => {
    return false;
    const messages = {
        error: "Vui lòng nhập đủ thông tin trên các trường bắt buộc",
        total: "Vui lòng nhập tổng sản lượng",
        exceedTotal: "Vui lòng nhập sản lượng vượt lớn hơn tổng sản lượng",
        quarter: "Vui lòng nhập ít nhất 1 quý",
        exceedNotEqual: "Vui lòng không nhập sản lượng vượt bằng nhau",
        exceedOrdered: "Vui lòng nhập sản lượng vượt lớn hơn dòng trên",
        discount: "Vui lòng nhập chiết khấu",
    };

    for (const el of data) {
        if (!el.committedLineSub?.filter((i) => i.status != "D")?.length) {
            return notifyAndReturn("Vui lòng thêm điều kiện sản lượng");
        }
        for (const element of el.committedLineSub) {
            if (!element.brandIds || !element.industryId)
                return notifyAndReturn(messages.error);

            if (!element.total && el.committedType !== "P")
                return notifyAndReturn(messages.total);

            if (
                el.committedType != "P" &&
                hasInvalidExceed(element.total, element.committedLineSubSub)
            )
                return notifyAndReturn(messages.exceedTotal);

            if (!element.package && el.committedType === "P")
                return notifyAndReturn(messages.total);

            if (
                el.committedType === "P" &&
                hasInvalidExceed(element.package, element.committedLineSubSub)
            )
                return notifyAndReturn(messages.exceedTotal);

            if (el.committedType === "Q" && !hasQuarterData(element))
                return notifyAndReturn(messages.quarter);

            if (hasDuplicateOrUnorderedExceed(element.committedLineSubSub)) return true;

            if (!el.discount && el.committedType === "P") {
                return notifyAndReturn(messages.discount);
            }

            if (!el.discountYear && el.committedType === "Y") {
                return notifyAndReturn(messages.discount);
            }

            if (!el.discount && el.committedType === "Q") {
                return notifyAndReturn(messages.discount);
            }
        }
    }

    return false;

    function notifyAndReturn(message) {
        FunctionGlobal.$notify("E", message, toast);
        return true;
    }

    function hasInvalidExceed(threshold, subSubData) {
        return subSubData
            .filter((e) => e.status != "D")
            .some((elx) => elx.outPut > 0 && elx.outPut <= threshold);
    }

    function hasQuarterData(element) {
        return (
            element.quarter1 || element.quarter2 || element.quarter3 || element.quarter4
        );
    }

    function hasDuplicateOrUnorderedExceed(subSubData) {
        for (let i = 0; i < subSubData.filter((e) => e.status != "D").length; i++) {
            const current = subSubData[i];
            if (current.outPut) {
                if (
                    subSubData
                        .filter((e) => e.status != "D")
                        .filter((elx) => elx.outPut === current.outPut).length > 1
                )
                    return notifyAndReturn(messages.exceedNotEqual);
                if (i > 0 && current.outPut < subSubData[i - 1].outPut)
                    return notifyAndReturn(messages.exceedOrdered);
            }
        }
        return false;
    }
};

const onFilter = async (isClear = false) => {
    const queryString = conditionHandler.getQuery(filterStore.filters);
    if (isClear) {
        dataTable.value.skip = 0;
    }
    dataCommited.value = await GetCommited(queryString);
};

const onPageChange = (event) => {
    dataTable.value.skip = event.page;
    dataTable.value.limit = event.rows;

    onFilter();
};

const clearFilter = () => {
    filterStore.resetFilters();
    onFilter(true);
};

const onModalHide = () => {
    router.replace({
        name: router.name,
        query: null,
    });
};

const excludeKeys = [
    "quarter1",
    "quarter2",
    "quarter3",
    "quarter4",
    "month1",
    "month2",
    "month3",
    "month4",
    "month5",
    "month6",
    "month7",
    "month8",
    "month9",
    "month10",
    "month11",
    "month12",
    "total",
];
const changeTotal = (data, field) => {
    if (!excludeKeys.includes(field)) return;
    if (field === "total") {
        excludeKeys
            .filter((el) => el != "total")
            .forEach((element) => {
                data[element] = 0;
            });
    }
};

const changeCal = (data, field) => {
    if (!excludeKeys.includes(field)) return;
    if (field === "total") return;
    const totalC = Object.keys(data)
        .filter((key) => excludeKeys.filter((el) => el != "total").includes(key))
        .reduce((sum, key) => sum + data[key], 0);
    data.total = totalC;
};

const getHierarchy = async () => {
    try {
        const { data } = await API.get("Item/hierarchy?cardId=");
        if (data) Hierarchy.value = data.items;
    } catch (error) {
        Hierarchy.value = [];
    }
};

const checkbrand = (el, brandIds) => {
    if (!el.select) return true;
    if (el.select) {
        if (brandIds) {
            if (brandIds.includes(el.id)) return true;
        }
    }
};

const onUploadCommitted = (event) => {
    // const file = event.files[0];
    // if (!file) return;
    // const workbook = new ExcelJS.Workbook();
    // const reader = new FileReader();
    // reader.onload = async (e) => {
    //   await workbook.xlsx.load(e.target.result);
    //   const worksheet = workbook.getWorksheet(1);
    //   const data = [];
    //   let headers = [];
    //   worksheet.eachRow((row, rowNumber) => {
    //     const rowValues = row.values.slice(1);
    //     if (rowNumber === 1) {
    //       headers = rowValues;
    //     } else {
    //       const rowObject = {};
    //       headers.forEach((header, index) => {
    //         rowObject[header] = rowValues[index] || null;
    //       });
    //       data.push(rowObject);
    //     }
    //   });
    // };
    // reader.readAsArrayBuffer(file);
};

const getIndustry = async () => {
    try {
        const { data } = await API.get("Industry/getall");
        Industry.value = data;
    } catch (error) {
        Industry.value = [];
    } finally {
        IndustryClone.value = JSON.stringify(Industry.value);
    }
};

getIndustry();

const setAllStatus = (obj, newStatus) => {
    if (typeof obj === "object" && obj !== null) {
        if ("status" in obj) {
            obj.status = newStatus;
        }
        for (const key in obj) {
            if (Array.isArray(obj[key])) {
                obj[key].forEach((item) => setAllStatus(item, newStatus));
            } else if (typeof obj[key] === "object") {
                setAllStatus(obj[key], newStatus);
            }
        }
    }
};

watch(
    () => PayloadData.value.committedLine,
    (newVal) => {
        Industry.value = JSON.parse(IndustryClone.value);
        newVal
            .filter((el) => el.status !== "D")
            .forEach((el) => {
                el.committedLineSub.forEach((e) => {
                    if (e.industryId && e.brandIds) {
                        const brands = Industry.value.find(
                            (item) => item.id === e.industryId
                        );
                        if (brands != undefined) {
                            brands.brands.forEach((brand) => {
                                if (e.brandIds.includes(brand.id)) brand.select = true;
                            });
                        }
                    }
                });
            });
    },
    { deep: true }
);

const getStatus = (key) => {
    const data = [
        {
            label: t('body.status.DXN'),
            code: "A",
            type: "primary",
        },
        {
            label: t('body.status.CXN'),
            code: "P",
            type: "warning",
        },
        {
            label: t('body.status.HUY'),
            code: "R",
            type: "danger",
        },
        {
            label: t('body.status.DH'),
            code: "C",
            type: "secondary",
        },
        {
            label: t('body.status.draft'),
            code: "D",
            type: "info",
        },
    ];
    return (
        data.find((el) => el.code == key) || {
            label: t('body.status.pending'),
            code: "P",
            type: "warning",
        }
    );
};

const debounceF = debounce(onFilter, 1000);

const validateQuarter = (quarter, date) => {
    const currentMonth = new Date(date).getMonth();
    const currentQuarter = Math.floor(currentMonth / 3) + 1;
    return quarter < currentQuarter;
};
const validateMonth = (month, date) => {
    const currentMonth = new Date(date).getMonth() + 1;
    return month < currentMonth;
};

const disabledField = (type, date, sp) => {
    if (type.startsWith("month")) {
        const check = validateMonth(parseInt(type.replace(/\D/g, ""), 10), date);
        if (check) {
            sp[type] = 0;
        }
        return check;
    }

    if (type.startsWith("quarter")) {
        const check = validateQuarter(parseInt(type.replace(/\D/g, ""), 10), date);
        if (check) {
            sp[type] = 0;
        }
        return check;
    }
};

const checkQuarter = (sp, type) => {
    if (type === "Y") {
        const q = [
            "month1",
            "month2",
            "month3",
            "month4",
            "month5",
            "month6",
            "month7",
            "month8",
            "month9",
            "month10",
            "month11",
            "month12",
        ];
        let count = 0;
        q.forEach((e) => {
            if (sp[e]) {
                count += 1;
            }
        });
        return count >= 1;
    }

    if (type === "Q") {
        const q = ["quarter1", "quarter2", "quarter3", "quarter4"];
        let count = 0;
        q.forEach((e) => {
            if (sp[e]) {
                count += 1;
            }
        });
        return count >= 3;
    }

    return true;
};

import { useHierarchyStore } from "../../../Pinia/hierarchyStore";
const hierarchy = useHierarchyStore();
const getItemTypeOption = (indIds, brandIds) => {
    const result = [];
    // hierarchy.getIndustryOptions();
    return result;
};

const getOptions = (data, field) => {
    let result = [];
    // logic

    switch (field) {
        case "industryId":
            result = Industry.value;
            break;
        case "brandIds":
            result = Industry.value.find((el) => el.id == data.industryId)?.brands || [];
            break;
        case "itemTypeIds":
            result = hierarchy.getItemTypeOptions(
                data.brandIds || [],
                data.industryId ? [data.industryId] : []
            );
            break;
        default:
            break;
    }

    return result;
};

const checkSelected = (dt, el, data, field) => {
    let result = true;
    // logic
    switch (field) {
        case "brandIds":
            // let sameRows = dt.filter(el => el.industryId == data.industryId && el.brandIds.length)
            // if(sameRows.length){
            //     const selectedBrand = sameRows.map(el => el.brandIds).flat()
            //     if(selectedBrand.includes(el.id)){
            //         result = false;
            //     }
            // }
            // if(data.brandIds.includes(el.id)){
            //     result = true;
            // }
            break;
        case "itemTypeIds":
            break;
        default:
            break;
    }

    return result;
};

// Industry.find(
//     (el) => el.id == data.industryId
// )?.brands.filter((b) => {
//     return checkbrand(b, data.brandIds);
// }) || []
</script>
<template>
    <div>
        <div class="flex gap-3">
            <div class="col-9">
                <h4 class="font-bold m-0">Danh sách cam kết sản lượng</h4>
            </div>
            <div class="col-3 flex justify-content-end gap-3">
                <!-- <FileUpload
          mode="basic"
          accept=".csv, .xls, .xlsx"
          chooseLabel="Nhập từ excel"
          class="bg-blue-500 border-0"
          :maxFileSize="1000000"
          @select="onUploadCommitted"
        /> -->
                <ImportExcel></ImportExcel>
                <Button @click="openConfirm()" icon="pi pi-plus" label="Tạo mới" />
            </div>
        </div>
        <div class="card flex flex-column gap-4 mt-3 p-2">
            <DataTable
                :value="dataCommited"
                v-model:filters="filterStore.filters"
                showGridlines
                scrollable
                tableStyle="min-width: 50rem; max-width: 100%"
                header="surface-200"
                lazy
                stripedRows
                paginator
                :rows="dataTable?.limit"
                :page="dataTable?.skip"
                :first="dataTable?.skip * dataTable?.limit"
                :totalRecords="dataTable?.total"
                @page="onPageChange($event)"
                :rowsPerPageOptions="[10, 20, 30]"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                currentPageReportTemplate="Hiển thị {first} - {last} trên tổng {totalRecords} cam kết sản lượng"
                filterDisplay="menu"
                :filterLocale="'vi'"
                @filter="onFilter"
            >
                <template #empty>
                    <div class="p-2 text-center">
                        Không tìm thấy danh sách cam kết phù hợp
                    </div>
                </template>
                <template #header>
                    <div class="flex justify-content-between">
                        <IconField iconPosition="left">
                            <InputText
                                placeholder="Nhập từ khóa tìm kiếm"
                                v-model="filterStore.filters['global'].value"
                                @input="debounceF"
                            />
                            <InputIcon>
                                <i class="pi pi-search" @click="onFilter()" />
                            </InputIcon>
                        </IconField>
                        <Button
                            type="button"
                            icon="pi pi-filter-slash"
                            label="Xóa bộ lọc"
                            outlined
                            @click="clearFilter()"
                        />
                    </div>
                </template>
                <Column header="#">
                    <template #body="{ index }">
                        <span>{{ index + 1 }}</span>
                    </template>
                </Column>
                <Column field="committedCode" header="Mã cam kết">
                    <template #body="{ data }">
                        <span
                            class="text-primary font-semibold hover:underline cursor-pointer"
                            @click="OpenDetail(data.id)"
                        >
                            <!-- <i class="pi pi-arrow-right mr-2"></i> -->
                            <span>{{ data.committedCode }}</span>
                        </span>
                    </template>
                </Column>
                <Column field="committedName" header="Tên cam kết"></Column>
                <Column field="cardName" header="Tên khách hàng">
                    <template #body="sp">
                        <router-link
                            class="text-primary font-semibold hover:underline"
                            :to="'/agen-man/agency-category/' + sp.data.cardId"
                        >
                            <!-- <i class="pi pi-arrow-right mr-2"></i> -->
                            <span>{{ sp.data?.cardName }}</span></router-link
                        >
                    </template>
                </Column>
                <Column
                    field="committedYear.Year"
                    :showFilterMatchModes="false"
                    header="Thời gian"
                    class="w-7rem"
                >
                    <template #body="sp">{{
                        new Date(sp.data?.committedYear).getFullYear()
                    }}</template>
                    <template #filter="{ filterModel }">
                        <InputNumber
                            v-model="filterModel.value"
                            placeholder="Nhập thời gian"
                        >
                        </InputNumber>
                    </template>
                </Column>
                <Column field="cardName" header="Người tạo">
                    <template #body="{ data }">
                        <span>{{ data.creator?.fullName || "" }}</span>
                    </template>
                </Column>
                <Column
                    field="docStatus"
                    header="Trạng thái"
                    :showFilterMatchModes="false"
                >
                    <template #body="sp">
                        <Tag
                            :value="getStatus(sp.data?.docStatus).label"
                            :severity="getStatus(sp.data?.docStatus).type"
                        ></Tag>
                    </template>
                    <template #filter="{ filterModel }">
                        <MultiSelect
                            v-model="filterModel.value"
                            :options="[
                                { name: 'Đang chờ', code: 'P' },
                                { name: 'Nháp', code: 'D' },
                                { name: 'Đã hủy', code: 'C' },
                                { name: 'Đã xác nhận', code: 'A' },
                            ]"
                            optionLabel="name"
                            optionValue="code"
                            placeholder="Chọn trạng thái"
                            class="p-column-filter"
                            showClear
                        >
                        </MultiSelect>
                    </template>
                </Column>
                <Column header="Hành động" style="width: 5rem">
                    <template #body="slotProp">
                        <div class="flex gap-2">
                            <Button
                                v-if="['D'].includes(slotProp.data?.docStatus)"
                                icon="pi pi-trash"
                                severity="danger"
                                text
                                @click="openConfirmDlg(slotProp.data, 'D')"
                                v-tooltip.left="'Xóa'"
                            />
                            <Button
                                v-if="[''].includes(slotProp.data?.docStatus)"
                                icon="pi pi-ban"
                                severity="danger"
                                text
                                @click="openConfirmDlg(slotProp.data, 'C')"
                                v-tooltip.left="'Hủy'"
                            />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>
    <Dialog
        v-model:visible="confirmModal"
        modal
        :draggable="false"
        @hide="onModalHide"
        :header="
            PayloadData.id ? 'Chi tiết cam kết sản lượng' : 'Tạo mới cam kết sản lượng'
        "
        :style="{ width: '85%' }"
        maximizable
    >
        <div class="flex flex-column gap-4">
            <div class="card m-0 grid">
                <div class="col-6 flex flex-column gap-2">
                    <div class="flex">
                        <div class="w-20rem">
                            <strong>Mã cam kết</strong>
                        </div>
                        <InputText
                            class="w-full"
                            placeholder="Nhập mã cam kết"
                            v-model="PayloadData.committedCode"
                            :disabled="PayloadData.id"
                        ></InputText>
                    </div>
                    <div class="flex">
                        <div class="w-20rem">
                            <strong>Tên cam kết</strong><sup class="text-red-500">*</sup>
                        </div>
                        <InputText
                            class="w-full"
                            :disabled="PayloadData.docStatus == 'A'"
                            placeholder="Nhập tên cam kết"
                            v-model="PayloadData.committedName"
                            :invalid="submited && !PayloadData.committedName"
                        ></InputText>
                    </div>
                    <div class="flex">
                        <div class="w-20rem"><strong>Ghi chú</strong></div>
                        <Textarea
                            :disabled="PayloadData.docStatus == 'A'"
                            placeholder="Nhập ghi chú cam kết"
                            class="w-full"
                            v-model="PayloadData.committedDescription"
                            autoResize
                            :rows="4"
                        ></Textarea>
                    </div>
                </div>
                <div class="col-6 flex flex-column gap-2">
                    <div class="flex justify-content-between">
                        <div class="w-20rem">
                            <strong>Đối tượng áp dụng</strong>
                            <sup class="text-red-500">*</sup>
                        </div>
                        <div class="flex flex-column gap-2 w-full">
                            <div class="flex align-items-center gap-2">
                                <InputGroup>
                                    <Dropdown
                                        placeholder="Chọn đối tượng áp dụng"
                                        filter
                                        :disabled="
                                            ['A', 'R'].includes(PayloadData.docStatus)
                                        "
                                        :invalid="submited && !PayloadData.cardId"
                                        optionLabel="cardName"
                                        optionValue="id"
                                        :options="dataCustomer"
                                        v-model="PayloadData.cardId"
                                    >
                                    </Dropdown>
                                    <Button
                                        icon="pi pi-user"
                                        outlined
                                        severity="secondary"
                                    />
                                </InputGroup>
                            </div>
                        </div>
                    </div>
                    <div class="flex justify-content-between">
                        <label class="w-20rem">
                            <strong>Thời gian cam kết</strong>
                            <sup class="text-red-500">*</sup></label
                        >
                        <div class="flex gap-3 w-full">
                            <div class="flex flex-column w-full gap-2">
                                <Calendar
                                    :disabled="PayloadData.docStatus == 'A'"
                                    v-model="PayloadData.committedYear"
                                    :invalid="submited && !PayloadData.committedYear"
                                    view="year"
                                    dateFormat="yy"
                                    :minDate="new Date()"
                                />
                            </div>
                        </div>
                    </div>
                    <div v-if="PayloadData.id" class="flex mb-2">
                        <label class="font-bold w-20rem" for="">Trạng thái</label>
                        <div class="flex w-full">
                            <Tag
                                :value="getStatus(PayloadData.docStatus).label"
                                :severity="getStatus(PayloadData.docStatus).type"
                            ></Tag>
                        </div>
                    </div>
                    <div class="flex" v-if="PayloadData.docStatus == 'R'">
                        <label class="font-bold w-20rem" for="">Lý do</label>
                        <div class="w-full">{{ PayloadData.rejectReason }}</div>
                    </div>
                </div>
            </div>
            <div
                class="card flex flex-column gap-3"
                v-for="(item, index) of PayloadData.committedLine.filter(
                    (e) => e.status != 'D'
                )"
                :key="item"
            >
                <div class="flex gap-2 align-items-center">
                    <strong class="text-xl">Nội dung cam kết</strong>
                    <div v-if="PayloadData.docStatus != 'A'">
                        <Button
                            label="Xóa hình thức"
                            size="small"
                            outlined
                            severity="danger"
                            v-if="index"
                            @click="RemoveTypeCommit(item, index)"
                        />
                    </div>
                </div>
                <div class="grid">
                    <div class="col-6 gap-2">
                        <div class="flex align-items-center gap-3">
                            <span class=""
                                ><strong> Hình thức áp dụng</strong>
                                <sup class="text-red-500">*</sup></span
                            >
                            <Dropdown
                                :disabled="PayloadData.docStatus == 'A'"
                                optionLabel="name"
                                optionValue="code"
                                class="w-10rem"
                                :options="
                                    dataCommittedType.filter(
                                        (el) => !el.index || el.index == index + 1
                                    )
                                "
                                v-model="item.committedType"
                                placeholder="Chọn hình thức áp dụng"
                                @change="ChangeTypeCommit($event, index)"
                            />
                        </div>
                    </div>
                    <div class="col-12 p-2">
                        <DataTable
                            :value="item.committedLineSub.filter((e) => e.status != 'D')"
                            tableStyle="min-width: 50rem"
                            resizableColumns
                            columnResizeMode="fit"
                            v-model:expandedRows="expandedRows"
                            showGridlines=""
                        >
                            <Column style="width: 5rem">
                                <template #body="{ index, data }">
                                    <Button
                                        icon="pi pi-trash"
                                        @click="
                                            DeleteRows(item.committedLineSub, data, index)
                                        "
                                        text
                                        severity="danger"
                                        v-if="PayloadData.docStatus != 'A'"
                                    />
                                </template>
                            </Column>

                            <Column expander style="width: 5rem" />
                            <Column
                                v-for="col of DataStruct.structTable.filter(
                                    (el) =>
                                        !el.committedType ||
                                        el.committedType?.includes(item.committedType)
                                )"
                                :key="col.field"
                                :field="col.field"
                                :header="col.header"
                            >
                                <template #body="{ data, index }">
                                    <!-- {{ data[col.field] }} -->
                                    <Dropdown
                                        :disabled="PayloadData.docStatus == 'A'"
                                        v-if="col.typeValue == 'Dropdown'"
                                        :placeholder="col.placeholder"
                                        v-model="data[col.field]"
                                        :options="getOptions(data, col.field)"
                                        optionLabel="name"
                                        optionValue="id"
                                        emptyMessage="Không có dữ liệu"
                                        style="width: 10rem"
                                        @change="
                                            () => {
                                                data.brandIds = [];
                                                data.itemTypeIds = [];
                                            }
                                        "
                                        filter
                                        :invalid="submited && !data[col.field]"
                                    ></Dropdown>
                                    <MultiSelect
                                        :disabled="PayloadData.docStatus == 'A'"
                                        v-if="col.typeValue == 'MultiSelect'"
                                        :placeholder="col.placeholder"
                                        v-model="data[col.field]"
                                        :options="
                                            getOptions(data, col.field).filter((el) =>
                                                checkSelected(
                                                    item.committedLineSub.filter(
                                                        (e) => e.status != 'D'
                                                    ),
                                                    el,
                                                    data,
                                                    col.field
                                                )
                                            )
                                        "
                                        filter
                                        :optionLabel="
                                            col.field == 'itemTypeIds'
                                                ? 'itemTypeName'
                                                : 'name'
                                        "
                                        :optionValue="
                                            col.field == 'itemTypeIds'
                                                ? 'itemTypeId'
                                                : 'id'
                                        "
                                        emptyMessage="Không có dữ liệu"
                                        style="width: 10rem"
                                        :invalid="submited && !data[col.field]"
                                    >
                                    </MultiSelect>
                                    <InputNumber
                                        :disabled="
                                            PayloadData.docStatus == 'A' ||
                                            disabledField(
                                                col.field,
                                                PayloadData.committedYear,
                                                data
                                            )
                                        "
                                        v-if="col.typeValue == 'InputNumber'"
                                        :placeholder="col.placeholder"
                                        style="width: 10rem"
                                        v-model="data[col.field]"
                                        @input="changeTotal(data, col.field)"
                                        @update:modelValue="changeCal(data, col.field)"
                                        :min="0"
                                        :invalid="
                                            submited &&
                                            !data[col.field] &&
                                            ['package', 'total'].includes(col.field)
                                        "
                                    ></InputNumber>

                                    <div
                                        class="flex align-items-center gap-2"
                                        v-if="col.typeValue == 'Mixed'"
                                    >
                                        <InputNumber
                                            :disabled="PayloadData.docStatus == 'A'"
                                            v-model="data[col.field]"
                                            style="width: 10rem"
                                            :placeholder="col.placeholder"
                                            suffix="%"
                                            :min="
                                                item.committedType == 'Q' &&
                                                [
                                                    'discountYear',
                                                    'nineMonthDiscount',
                                                ].includes(col.field)
                                                    ? 0
                                                    : 1
                                            "
                                            :max="100"
                                            :invalid="
                                                item.committedType == 'Q' &&
                                                col.field == 'discountYear'
                                                    ? false
                                                    : col.require &&
                                                      submited &&
                                                      !data[col.field]
                                            "
                                        ></InputNumber>
                                        <div class="flex gap-2">
                                            <Checkbox
                                                :disabled="PayloadData.docStatus == 'A'"
                                                v-model="data[col.sub_field]"
                                                binary
                                            >
                                            </Checkbox>
                                            <span>Quy ra hàng</span>
                                        </div>
                                    </div>
                                </template>
                            </Column>
                            <template #expansion="sp">
                                <DataTable
                                    :value="
                                        sp.data.committedLineSubSub.filter(
                                            (e) => e.status != 'D'
                                        )
                                    "
                                    tableStyle="width: 42rem"
                                    resizableColumns
                                    columnResizeMode="fit"
                                >
                                    <Column field="code" header="#" class="w-1rem">
                                        <template #body="{ index }">
                                            <span>{{ index + 1 }}</span>
                                        </template>
                                    </Column>
                                    <Column field="name" header="Sản lượng vượt">
                                        <template #body="{ data }">
                                            <InputNumber
                                                :disabled="PayloadData.docStatus == 'A'"
                                                placeholder="Nhập sản lượng vượt"
                                                v-model="data.outPut"
                                                :min="0"
                                                :invalid="
                                                    submited &&
                                                    !data.outPut &&
                                                    item.committedType != 'Q'
                                                "
                                            >
                                            </InputNumber>
                                        </template>
                                    </Column>
                                    <Column
                                        field="category"
                                        header="Chiết khấu"
                                        class="w-20rem"
                                    >
                                        <template #body="{ data }">
                                            <div class="flex gap-2 align-items-center">
                                                <InputNumber
                                                    placeholder="Nhập chiết khấu"
                                                    v-model="data.discount"
                                                    :disabled="
                                                        PayloadData.docStatus == 'A'
                                                    "
                                                    suffix="%"
                                                    :min="1"
                                                    :max="100"
                                                    :invalid="
                                                        submited &&
                                                        !data.discount &&
                                                        data.isConvert
                                                    "
                                                ></InputNumber>
                                                <div class="flex gap-2">
                                                    <Checkbox
                                                        binary
                                                        v-model="data.isConvert"
                                                        :disabled="
                                                            PayloadData.docStatus == 'A'
                                                        "
                                                    ></Checkbox>
                                                    <span>Quy ra hàng</span>
                                                </div>
                                            </div>
                                        </template>
                                    </Column>
                                    <Column>
                                        <template #body="{ index, data }">
                                            <Button
                                                v-if="PayloadData.docStatus != 'A'"
                                                icon="pi pi-trash"
                                                @click="
                                                    DeleteRows(
                                                        sp.data.committedLineSubSub,
                                                        data,
                                                        index
                                                    )
                                                "
                                                text
                                                severity="danger"
                                            />
                                        </template>
                                    </Column>
                                    <template #footer>
                                        <Button
                                            v-if="PayloadData.docStatus != 'A'"
                                            label="Thêm dòng"
                                            icon="pi pi-plus-circle"
                                            size="small"
                                            @click="
                                                AddRows(sp.data.committedLineSubSub, true)
                                            "
                                            outlined
                                        />
                                    </template>
                                </DataTable>
                            </template>
                            <template #footer>
                                <Button
                                    label="Thêm điều kiện"
                                    icon="pi pi-plus-circle"
                                    v-if="PayloadData.docStatus != 'A'"
                                    @click="AddRows(item.committedLineSub)"
                                    size="small"
                                    outlined
                                />
                            </template>
                        </DataTable>
                    </div>
                </div>
            </div>
        </div>
        <div class="flex justify-content-start mt-3">
            <Button
                outlined
                icon="pi pi-plus-circle"
                label=" Thêm mới hình thức áp dụng"
                text
                size="small"
                v-if="PayloadData.docStatus != 'A'"
                :disabled="PayloadData.committedLine.length > 2"
                @click="AddForm()"
            />
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button
                    label="Đóng"
                    icon="pi pi-times"
                    severity="secondary"
                    @click="confirmModal = false"
                />
                <Button
                    label="Lưu nháp"
                    icon="pi pi-save"
                    v-if="!['A', 'P', 'R', 'C'].includes(PayloadData.docStatus)"
                    severity="info"
                    @click="saveCommited('D')"
                />
                <Button
                    v-if="['D', 'R'].includes(PayloadData.docStatus) || !PayloadData.id"
                    :label="PayloadData.docStatus == 'P' ? 'Gửi lại' : 'Gửi'"
                    @click="saveCommited('P')"
                    icon="pi pi-send"
                />
            </div>
        </template>
    </Dialog>
    <Dialog
        v-model:visible="visibleConfirmDlg"
        modal
        :draggable="false"
        :header="`Xác nhận ${action == 'C' ? 'hủy' : 'xóa'}`"
        :style="{ width: '25rem' }"
    >
        <div class="flex justify-content-center">
            <span
                >Xác nhận {{ action == "C" ? "hủy" : "xóa" }} mã cam kết
                <strong>{{ rowSelect?.committedCode }}</strong></span
            >
        </div>
        <template #footer>
            <div class="flex justify-content-end gap-2">
                <Button
                    type="button"
                    label="Hủy"
                    severity="secondary"
                    @click="visibleConfirmDlg = false"
                />
                <Button
                    type="button"
                    label="Xác nhận"
                    severity="danger"
                    @click="onClickConfirm(rowSelect)"
                />
            </div>
        </template>
    </Dialog>
    <Loading v-if="loadding"></Loading>
</template>
<style scoped>
.bg_ {
    background: none;
    border: none;
}

:deep(.p-inputnumber-input) {
    width: 100%;
}
</style>
