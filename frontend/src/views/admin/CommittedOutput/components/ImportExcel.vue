<template>
    <Button
        v-if="0"
        label="Nhập từ Excel"
        @click="visible = true"
        severity="help"
        icon="pi pi-file-import"
    />

    <Dialog
        v-model:visible="visible"
        modal
        header="Nhập liệu từ sản lượng cam kết "
        :style="{ width: '70%' }"
    >
        <FileUpload :accept="accept" customUpload @select="onSelectedFiles">
            <template #header="{ chooseCallback, clearCallback, files }">
                <div
                    class="flex gap-5 justify-content-end align-items-center flex-grow-1"
                >
                    <div class="flex-grow-1" v-if="files[0]">
                        <div
                            class="card p-0 mb-0 pl-3 flex justify-content-between align-items-center"
                        >
                            <span class="font-semibold flex align-items-center gap-2">
                                <i class="pi pi-file"></i>
                                {{ files[0]?.name }}
                                <Tag icon="pi pi-check"></Tag>
                            </span>
                            <Button
                                text
                                icon="pi pi-times"
                                severity="danger"
                                @click="clearCallback"
                            />
                        </div>
                    </div>
                    <div class="flex gap-2">
                        <Button
                            label="Chọn file"
                            @click="chooseCallback"
                            icon="pi pi-plus"
                            severity="info"
                        />
                        <Button
                            label="Tải lên"
                            @click="SavePromotion(dataImport)"
                            icon="pi pi-upload"
                            :disabled="!files[0]"
                        />
                    </div>
                </div>
            </template>
            <template #content>
                <DataTable showGridlines :value="dataImport">
                    <Column header="#">
                        <template #body="{ index }">
                            {{ index + 1 }}
                        </template>
                    </Column>
                    <Column header="Mã cam kết" field="committedCode"></Column>
                    <Column header="Tên cam kết" field="committedName"></Column>
                    <Column header="Trạng thái" field="docStatus">
                        <template #body="sp">
                            <Tag
                                :value="setStatus(sp.data.docStatus).label"
                                :severity="setStatus(sp.data.docStatus).type"
                            ></Tag>
                        </template>
                    </Column>
                </DataTable>
            </template>
        </FileUpload>
    </Dialog>
</template>

<script setup>
import { ref } from "vue";
import readXlsxFile from "read-excel-file";
import API from "@/api/api-main";
import { forEach, merge } from "lodash";
import { useGlobal } from "@/services/useGlobal";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const { toast, FunctionGlobal } = useGlobal();
const PayloadData = {
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
            fatherId: 0,
            committedType: "",
            status: "",
            committedLineSub: [
                {
                    id: 0,
                    fatherId: 0,
                    industryId: 0,
                    brandIds: 0,
                    quarter1: 0,
                    quarter2: 0,
                    quarter3: 0,
                    quarter4: 0,
                    // package: 0,
                    month1: 0,
                    month2: 0,
                    month3: 0,
                    month4: 0,
                    month5: 0,
                    month6: 0,
                    month7: 0,
                    month8: 0,
                    month9: 0,
                    month10: 0,
                    month11: 0,
                    month12: 0,
                    total: 0,
                    discount: 1,
                    isConvert: false,
                    discountMonth: 1,
                    isCvMonth: false,
                    nineMonthDiscount: 0,
                    isCvNineMonth: false,
                    discountYear: 1,
                    isCvYear: false,
                    status: "",
                    committedLineSubSub: [
                        {
                            id: 0,
                            fatherId: 0,
                            outPut: 0,
                            discount: 1,
                            isConvert: false,
                            status: "",
                        },
                    ],
                },
            ],
        },
    ],
};

const setStatus = (key) => {
    const data = [
        {
            label: "Đã xác nhận",
            code: "A",
            type: "primary",
        },
        {
            label: "Đang chờ",
            code: "P",
            type: "warning",
        },
        {
            label: "Đã hủy",
            code: "R",
            type: "danger",
        },
        {
            label: "Nháp",
            code: "D",
            type: "info",
        },
    ];
    return data.find((el) => el.code == key);
};

const dataImport = ref(null);
const visible = ref(false);
const accept =
    ".csv, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel";

const onSelectedFiles = async (event) => {
    await readExcelFile(event.files[0]);
};

const readExcelFile = async (file) => {
    try {
        const sheetsData = await readXlsxFile(file, { getSheets: true });
        const sheetsName = Object.values(sheetsData).map((sheet) => sheet.name);

        const allSheetsData = await Promise.all(
            sheetsName.map((sheet) => readXlsxFile(file, { sheet }))
        );

        const commited_general = allSheetsData[0];
        const commited_content = allSheetsData[1];
        const commited_detail_content = allSheetsData[2];
        const commited_detail_applied_method = allSheetsData[3];

        let Obj_commited_general = arrToObj(commited_general);
        let Obj_commited_content = arrToObj(commited_content);
        let Obj_commited_detail_content = arrToObj(commited_detail_content);
        let Obj_commited_detail_applited_method = arrToObj(
            commited_detail_applied_method
        );

        await findCommitted(Obj_commited_general, Obj_commited_content);

        await findCommittedSub(Obj_commited_content, Obj_commited_detail_content);

        await findCommittedSubSub(
            Obj_commited_detail_content,
            Obj_commited_detail_applited_method
        );

        let result = [];
        for (let value of Obj_commited_general) {
            const res = merge({}, PayloadData, value);
            result.push(res);
        }
        if (result.length > 0) {
            dataImport.value = result;
        }
        // SavePromotion(result);
    } catch (error) {
        console.error("Error reading the Excel file:", error);
    } finally {
    }
};

const arrToObj = (arr) => {
    const keys = arr[0];
    const data = arr.splice(1);
    const result = data.map((values) => {
        return keys.reduce((acc, key, index) => {
            key = extractString(key);
            if (values[index] !== null && key !== null) {
                acc[key.replace(/\s+/g, "")] = values[index];
            }
            return acc;
        }, {});
    });
    return result;
};

const findCommitted = async (commited, commited_sub) => {
    for (let el of commited) {
        const cardId = await getCustomer(el.cardId);
        const time = new Date();
        time.setFullYear(el.committedYear);
        el.committedYear = time.toISOString();
        el.cardId = cardId;
        el.committedLine = commited_sub.filter((e) => e.fatherId == el.id);

        delete el.id;
        if (el.committedLine.length) {
            el.committedLine.forEach((e) => {
                delete e.fatherId;
            });
        }
    }
};

const findCommittedSub = async (commitedSub, commitedSubSub) => {
    await Promise.all(
        commitedSub.map(async (el) => {
            el.committedLineSub = commitedSubSub.filter((e) => e.fatherId == el.id);

            await Promise.all(
                el.committedLineSub.map(async (sub) => {
                    const industryId = await getIndustry(sub.industryId);
                    const brandIds = await getBrand(sub.brandIds);
                    sub.industryId = industryId; // Gán industryId cho sub
                    sub.brandIds = brandIds;
                })
            );
            delete el.id;
            if (el.committedLineSub.length) {
                el.committedLineSub.forEach((e) => {
                    delete e.fatherId;
                });
            }
        })
    );
};

const findCommittedSubSub = async (commitedSubSub, commitedSubSubSub) => {
    commitedSubSub.map((el) => {
        el.committedLineSubSub = commitedSubSubSub.filter((e) => e.fatherId == el.id);

        delete el.id;
        if (el.committedLineSubSub.length) {
            el.committedLineSubSub.forEach((e) => {
                delete e.id;
                delete e.fatherId;
            });
        }
    });
};

const getCustomer = async (data) => {
    try {
        const response = await API.get(`Customer?search=${data}`);
        if (response.data.items) {
            let data = response.data.items;

            // return {
            //   customerId: data[0]?.id,
            //   customerName: data[0]?.cardName,
            //   customerCode: data[0]?.cardCode,
            // };
            return data[0]?.id;
        }

        return null;
    } catch (error) {
        console.error(`Error fetching customer ${error} `);
        return null;
    }
};

const getIndustry = async (data) => {
    try {
        const response = await API.get(`Industry/search/${data}`);
        if (response.data) {
            let data = response.data[0];
            return data.id;
        }
        return null;
    } catch (error) {
        console.error(`Error fetching industry: ${error}`);
        return null;
    }
};

const getBrand = async (data) => {
    const arrCode = data.split(",").map((code) => code.trim());

    const brandPromises = arrCode.map(async (code) => {
        try {
            const response = await API.get(`Brand/search/${code}`);
            if (response.data) {
                return response.data[0].id;
            }
            return null;
        } catch (error) {
            console.error(`Error fetching brand: ${error}`);
            return null;
        }
    });

    const brands = await Promise.all(brandPromises);
    return brands;
};

const SavePromotion = async (payload) => {
    for (let data of payload) {
        try {
            const response = await API.add("Commited", data);

            if (response) {
                FunctionGlobal.$notify("S", t('Custom.addnewSuccess'), toast);
                data.statusImport = true;
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            }

        } catch (error) {
            FunctionGlobal.$notify("E", error.response.data.errors, toast);
        }
    }

    dataImport.value = [];
};

const extractString = (input) => {
    const regex = /\((.*?)\)/; // Tìm chuỗi trong dấu ngoặc đơn
    const match = input.match(regex); // Sử dụng match để lấy kết quả

    return match ? match[1] : null; // Nếu tìm thấy, trả về chuỗi, ngược lại trả về null
};
</script>
