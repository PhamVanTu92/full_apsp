<template>
  <Button
    icon="pi pi-file-import"
    label="Nhập excel"
    severity="help"
    @click="onClickOpenDlg"
  />
  <Dialog
    v-model:visible="visible"
    header="Nhập liệu tập chương trình khuyến mãi"
    modal
    class="w-8 md:w-7 lg:w-6"
  >
    <FileUpload
      @select="onSelectedFiles"
      :accept="accept"
      customUpload
      @uploader="customBase64Uploader"
      :pt:buttonbar:class="'border-1 border-200'"
    >
      <template #header="{ chooseCallback, clearCallback, files }">
        <div class="flex gap-5 justify-content-end align-items-center flex-grow-1">
          <div class="flex-grow-1" v-if="files[0]">
            <div
              class="card p-0 mb-0 pl-3 flex justify-content-between align-items-center"
            >
              <span class="font-semibold flex align-items-center gap-2">
                <i class="pi pi-file"></i>
                {{ files[0]?.name }}
                <Tag icon="pi pi-check" value="validate"></Tag>
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
              icon="pi pi-upload"
              @click="SavePromotion(dataImport)"
              :disabled="!files[0]"
            />
          </div>
        </div>
      </template>
      <template #content>
        <DataTable showGridlines class="h-20rem" :value="dataImport">
          <Column header="#" class="w-3rem">
            <template #body="{ index }">{{ index + 1 }}</template>
          </Column>
          <Column field="promotionCode" header="Mã chương trình"></Column>
          <Column field="promotionName" header="Tên chương trình"></Column>
          <Column header="Trạng thái">
            <template #body="{ data }">
              <span v-if="!data.submited">Đang xử lý</span>
              <span v-if="data.submited">{{
                data.statusImport ? "Import thành công" : "Import thất bại"
              }}</span>
            </template>
          </Column>
        </DataTable>
      </template>
      <template #empty>
        <div class="flex justify-content-center align-items-center h-20rem">
          <p class="text-center">Kéo, thả file Excel để nhập dữ liệu</p>
        </div>
      </template>
    </FileUpload>
    <template #footer>
      <div class="flex flex-grow-1">
        <Button
          icon="pi pi-download"
          label="Tải xuống file nhập liệu mẫu"
          outlined
        />
      </div>
    </template>
  </Dialog>
  <Loading v-if="loading"></Loading>
</template>

<script setup>
import { ref, onBeforeMount } from "vue";
import readXlsxFile from "read-excel-file";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";

const { toast, FunctionGlobal } = useGlobal();
const visible = ref(false);
const loading = ref(false);
const submited = ref(false);
const dataImport = ref(null);

const onClickOpenDlg = () => {
  visible.value = true;
};

onBeforeMount(async () => {});

const onSelectedFiles = async (event) => {
  await readExcelFile(event.files[0]);
};

const readExcelFile = async (file) => {
  try {
    loading.value = true;
    // Đọc tất cả các sheet trong file cùng lúc
    const sheetsData = await readXlsxFile(file, { getSheets: true });
    const sheetNames = Object.values(sheetsData).map((sheet) => sheet.name);

    // Đọc dữ liệu từ tất cả các sheet
    const allSheetsData = await Promise.all(
      sheetNames.map((sheet) => readXlsxFile(file, { sheet }))
    );

    // Giả sử bạn muốn xử lý sheet đầu tiên và thứ hai
    const promotion = allSheetsData[0];
    const promotionLine = allSheetsData[1];
    const promotionLineSub = allSheetsData[2];
    const promotionLineSubSub = allSheetsData[3];





    const ObjpromotionLineSubSub = arrToObj(promotionLineSubSub);
    const ObjpromotionLineSub = arrToObj(promotionLineSub);
    const ObjpromotionLine = arrToObj(promotionLine);
    const Objpromotion = arrToObj(promotion) || null;




    findPromotionLineSub(ObjpromotionLineSub, ObjpromotionLineSubSub);
    findPromotionLine(ObjpromotionLine, ObjpromotionLineSub);
    findPromotion(Objpromotion, ObjpromotionLine);

    if (Objpromotion) {
      dataImport.value = Objpromotion;
    }
    // SavePromotion(Objpromotion);

    // Tiến hành các xử lý khác nếu cần
  } catch (error) {
    console.error("Error reading the Excel file:", error);
  } finally {
    loading.value = false;
  }
};

const findPromotionLineSub = async (promotionLineSub, promotionLineSubSub) => {
  promotionLineSub.forEach((el) => {
    el.promotionLineSubSub = promotionLineSubSub.filter((e) => e.fatherId == el.id);
  });
  await Promise.all(
    promotionLineSub.map(async (el) => {
      el.promotionItemBuy = await getPromotionItemBuy(
        el.promotionItemBuy || [],
        el.itemType
      );
      el.promotionUnit = await getPromotionUnit(el.promotionUnit);
      el.promotionLineSubSub = promotionLineSubSub.filter((e) => e.fatherId == el.id);
      delete el.id;
      if (el.promotionLineSubSub.length) {
        await Promise.all(
          el.promotionLineSubSub.map(async (e) => {
            e.promotionSubItemAdd = await getPromotionItemBuy(
              e.promotionSubItemAdd || [],
              e.itemType
            );
            e.promotionSubUnit = await getPromotionUnit(e.promotionSubUnit);
            delete e.fatherId;
            delete e.id;
          })
        );
      }
    })
  );
};

const findPromotionLine = (promotionLine, promotionLineSub) => {
  promotionLine.forEach((el) => {
    el.promotionLineSub = promotionLineSub.filter((e) => e.fatherId == el.id);
    delete el.id;
    if (el.promotionLineSub.length) {
      el.promotionLineSub.forEach((e) => {
        delete e.fatherId;
      });
    }
  });
};

const findPromotion = async (promotion, promotionLine) => {
  await Promise.all(
    promotion.map(async (el) => {
      el.promotionCustomer = await getCustomer(el.promotionCustomer, el.typeCustomer);
      el.promotionIndustry = await getPromotionIndustry(el.promotionIndustry);
      el.promotionBrand = await getPromotionBrand(el.promotionBrand);
      el.promotionLine = promotionLine.filter((e) => e.fatherId === el.id);
      delete el.id;
      if (el.promotionLine.length) {
        el.promotionLine.forEach((e) => {
          delete e.fatherId;
        });
      }
    })
  );
};

const arrToObj = (arr) => {
  const keys = arr[0]; // Lấy phần tử đầu tiên làm key
  const data = arr.slice(1); // Lấy phần tử còn lại là data
  const result = data.map((values) => {
    return keys.reduce((acc, key, index) => {
      key = extractString(key);
      if (values[index] != null && key != null)
        acc[key.replace(/\s+/g, "")] = values[index];
      return acc;
    }, {});
  });
  return result;
};

const getCustomer = async (data, type) => {
  const ArrCardCode = data.split(",").map((code) => code.trim());

  const customerPromises = ArrCardCode.map(async (cardCode) => {
    try {
      const response = await API.get(`Customer?search=${cardCode}`);
      const { data } = response;
      if (data.items) {
        return {
          type: type,
          customerId: data?.items[0]?.id,
          CustomerCode: data?.items[0]?.cardCode,
          customerName: data?.items[0]?.cardName,
        };
      }
      return null;
    } catch (error) {
      console.error(`Error fetching customer with code ${cardCode}:`, error);
      return null;
    }
  });

  const customers = await Promise.all(customerPromises);
  return customers.filter(Boolean); // Loại bỏ các giá trị null
};

const getPromotionIndustry = async (data) => {
  if (data) {
    const ArrCode = data.split(",").map((code) => code.trim());

    const industryPromises = ArrCode.map(async (code) => {
      try {
        const response = await API.get(`Industry/search/${code}`);
        const { data } = response;
        if (data?.length) {
          return {
            industryId: data[0]?.id,
            industryName: data[0]?.name,
          };
        }
        return null;
      } catch (error) {
        console.error(`Error fetching industry with code ${code}:`, error);
        return null;
      }
    });
    const industry = await Promise.all(industryPromises);
    return industry.filter(Boolean); // Loại bỏ các giá trị null
  }
  return [];
};

const getPromotionBrand = async (data) => {
  if (data) {
    const ArrCode = data.split(",").map((code) => code.trim());

    const brandPromises = ArrCode.map(async (code) => {
      try {
        const response = await API.get(`Brand/search/${code}`);
        const { data } = response;
        if (data?.length) {
          return {
            brandId: data[0]?.id || 0,
            brandName: data[0]?.name || 0,
          };
        }
        return null;
      } catch (error) {
        console.error(`Error fetching industry with code ${code}:`, error);
        return null;
      }
    });

    const brand = await Promise.all(brandPromises);
    return brand.filter(Boolean); // Loại bỏ các giá trị null
  }
  return [];
};

const getPromotionUnit = async (data) => {
  if (data) {
    const ArrCode = data.split(",").map((code) => code.trim());

    const UnitPromises = ArrCode.map(async (code) => {
      try {
        const response = await API.get(`Packing/search/${code}`);
        const { data } = response;
        if (data?.length) {
          return {
            uomId: data[0]?.id || 0,
            uomName: data[0]?.name || null,
          };
        }
        return null;
      } catch (error) {
        console.error(`Error fetching Unit with code ${code}:`, error);
        return null;
      }
    });

    const Unit = await Promise.all(UnitPromises);
    return Unit.filter(Boolean); // Loại bỏ các giá trị null
  }
  return [];
};

const getPromotionItemBuy = async (data, type) => {
  if (data) {
    const ArrCode = data.split(",").map((code) => code.trim());

    const ItemBuyPromises = ArrCode.map(async (code) => {
      try {
        const url = type == "G" ? "ItemType/search/" : "Item?search=";
        const response = await API.get(`${url}${code}`);
        const { data } = response;
        if (data?.items?.length) {
          return {
            itemType: type || null,
            itemId: data.items[0].id || null,
            itemCode: data.items[0].itemCode || data?.items[0]?.code,
            itemName: data.items[0].itemName || data.items[0].name,
          };
        }
        return null;
      } catch (error) {
        console.error(`Error fetching ItemBuy with code ${code}:`, error);
        return null;
      }
    });

    const ItemBuy = await Promise.all(ItemBuyPromises);
    return ItemBuy.filter(Boolean); // Loại bỏ các giá trị null
  }
  return [];
};

const SavePromotion = async (payload) => {
  for (const data of payload) {
    data.submited = false;
    try {
      const res = await API.add("promotion/add", data);
      if (res) {
        FunctionGlobal.$notify("S", "Thêm mới thành công", toast);
        data.statusImport = true;
        setTimeout(() => {
          window.location.reload();
        }, 1000);
      }
    } catch (error) {
      data.statusImport = false;
      FunctionGlobal.$notify("E", error.response.data.errors, toast);
    } finally {
      data.submited = true;
    }
  }
};

const extractString = (input) => {
  const regex = /\((.*?)\)/; // Tìm chuỗi trong dấu ngoặc đơn
  const match = input.match(regex); // Sử dụng match để lấy kết quả

  return match ? match[1] : null; // Nếu tìm thấy, trả về chuỗi, ngược lại trả về null
};

const accept =
  ".csv, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel";
</script>

<style></style>
