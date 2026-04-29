<template>
  <div>
    <div class="grid">
      <div class="col-2">
        <div>
          <h3 class="font-bold m-0 mb-4">Khuyến mại</h3>
        </div>
        <div style="max-height: 77vh" class="overflow-auto">
          <div class="card shadow-1 p-0 mb-5">
            <div class="flex justify-content-between align-items-center py-2 px-3">
              <div><span class="m-0 font-bold text-base">Trạng thái</span></div>
              <div>
                <Button text icon="pi pi-angle-right" @click="createDate = !createDate" />
              </div>
            </div>
            <transition name="fade">
              <div class="p-3" v-if="createDate">
                <div class="flex align-items-center mb-3">
                  <RadioButton v-model="filterPrromotion.status" value="All"></RadioButton>
                  <label class="ml-2"> Tất cả </label>
                </div>
                <div class="flex align-items-center mb-3">
                  <RadioButton v-model="filterPrromotion.status" value="Active"></RadioButton>
                  <label class="ml-2"> Kích hoạt </label>
                </div>
                <div class="flex align-items-center mb-3">
                  <RadioButton v-model="filterPrromotion.status" value="NoActive"></RadioButton>
                  <label class="ml-2"> Chưa áp dụng</label>
                </div>
              </div>
            </transition>
          </div>
        </div>
      </div>
      <!-- Table -->
      <div class="col-10" :style="[styleSticky]">
        <div class="flex justify-content-between m-0 mb-3">
          <div class="w-9">
            <IconField iconPosition="left">
              <InputText type="text" placeholder="Tìm kiếm theo mã, tên đợt phát hành" @keydown.enter="searchGlobal()"
                class="w-full" v-model="keySearchProduct" />
              <InputIcon class="pi pi-search" />
            </IconField>
          </div>
          <div class="flex gap-2">
            <Button @click="openReleaseDialog()" icon="pi pi-plus" label="Khuyến mại"/>
            <Button icon="pi pi-align-justify" @click="op.toggle($event)" />
          </div>
        </div>
        <div style="max-height: 77vh" class="card p-2 shadow-1">
          <DataTable paginator :rows="rows" :page="pages" :totalRecords="totalRecords"
            currentPageReportTemplate="Hiển thị {first} - {last} trên tổng {totalRecords} sản phẩm" scrollable
            scrollHeight="70vh" stripedRows showGridlines v-model:expandedRows="expandedRows"
            @row-click="DetailPromotion" dataKey="id"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            :rowClass="rowClass" :rowsPerPageOptions="[10, 20, 30, 50]" @page="onCouponPageChange($event)"
            :value="DataPromotion" tableStyle="min-width: 50rem">
            <template #empty>
              <div class="p-4 text-center">
                <span class="p-column-title">Không tìm thấy kết quả nào phù hợp với từ khoá
                  {{ keySearchProduct ? '"' + keySearchProduct + '"' : "" }}</span>
              </div>
            </template>
            
            <Column v-for="col of selectedColumn" :key="col.field" :field="col.field" :header="col.header">
              <template #body="slotProps">
                <span v-if="col.type == 'text'">
                  {{ slotProps.data[col.field] }}
                </span>
                <span v-if="col.type == 'time'">
                  {{ format(slotProps.data[col.field], "dd/MM/yyyy") }}
                </span>
                
                <span v-if="col.type == 'doubleField'">
                  {{
                    getNameFormality(
                      slotProps.data[col.value[0]],
                      slotProps.data[col.value[1]]
                    )
                  }}
                </span>
                <span v-if="col.type == 'bool'">
                  {{ slotProps.data[col.field] == "Y" ? "Kích hoạt" : "Chưa kích hoạt" }}
                </span>
              </template>
            </Column>
          </DataTable>
        </div>
      </div>
    </div>
  </div>
  <OverlayPanel ref="op">
    <div class="flex flex-column flex-wrap gap-4">
      <div v-for="(item, index) in column" :key="index" class="flex align-items-center">
        <Checkbox v-model="selectedColumn" :value="item" @change="chooseColumn()"></Checkbox>
        <label class="ml-2"> {{ item.header }} </label>
      </div>
    </div>
  </OverlayPanel>
  <!-- Thêm mới khách hàng -->
  <Dialog position="top" :draggable="false" v-model:visible="releaseModal" modal :header="dataEdit.Status == 'Fix'
    ? 'Cập chương trình khuyến mại'
    : 'Thêm chương trình khuyến mại'
    " :style="{ width: '85%' }">
    <TabView>
      <TabPanel header="Thông tin khuyến mại">
        <div class="flex flex-column py-1">
          <div class="card">
            <div class="grid">
              <div class="col-6">
                <h6 class="font-semibold text-gray-700 m-0 mb-3">Thông tin</h6>
                <div class="flex align-items-center gap-2 mb-2">
                  <label class="font-semibold w-5 white-space-nowrap">Mã chương trình</label>
                  <InputText class="w-full" v-model="dataEdit.promotionCode" placeholder="Mã khuyến mại tự động sinh">
                  </InputText>
                </div>
                <div class="flex align-items-center gap-2 mb-2">
                  <label class="font-semibold w-5 white-space-nowrap">Tên chương trình</label>
                  <InputText class="w-full" v-model="dataEdit.promotionName"
                    :invalid="submited && !dataEdit.promotionName"></InputText>
                </div>
              </div>
              <div class="col-6">
                <div class="flex align-items-center gap-2 mb-3">
                  <label class="font-semibold">Trạng thái</label>
                  <div class="flex align-items-center">
                    <RadioButton v-model="dataEdit.promotionStatus" value="Y" />
                    <label for="ingredient1" class="ml-2">Kích hoạt</label>
                  </div>
                  <div class="flex align-items-center">
                    <RadioButton v-model="dataEdit.promotionStatus" value="N" />
                    <label for="ingredient1" class="ml-2">Chưa áp dụng</label>
                  </div>
                </div>
                <div>
                  <Textarea autoResize class="w-full" rows="5" cols="30" placeholder="Ghi chú"
                    v-model="dataEdit.promotionDescription" />
                </div>
              </div>
            </div>
          </div>
          <div class="card">
            <h6 class="font-semibold text-gray-700">Hình thức khuyến mại</h6>
            <div class="grid">
              <div class="col-6 mb-2">
                <label class="font-semibold w-5 white-space-nowrap">Khuyến mại theo</label>
                <Dropdown class="w-full mt-3" :options="saleOffData.promo" optionLabel="name" optionValue="id"
                  v-model="dataEdit.promotionType" @change="changePromo()"></Dropdown>
              </div>
              <div class="col-6 mb-2">
                <label class="font-semibold w-2 white-space-nowrap">Hình thức</label>
                <div class="flex align-items-center mt-3 gap-3">
                  <div class="w-full">
                    <Dropdown class="w-full" :options="checkCondition()" optionLabel="name" optionValue="id"
                      v-model="dataEdit.promotionSubType" @change="ResetCondition"></Dropdown>
                  </div>
                  <div class="flex items-center w-full"
                    v-if="['2-1', '2-2', '2-4'].includes(dataEdit.promotionSubType)">
                    <Checkbox v-model="checkCondition().filter((val) => {
                      return val.id == dataEdit.promotionSubType;
                    })[0].isMulti
                      " trueValue="Y" falseValue="N" :binary="true" />
                    <label for="ingredient1" class="ml-2">
                      {{
                        dataEdit.promotionSubType == "2-1"
                          ? "Hàng giảm giá không nhân theo SL mua."
                          : dataEdit.promotionSubType == "2-2"
                            ? "Hàng tặng không nhân theo số lượng mua."
                            : "Số lượng voucher không nhân theo SL mua. "
                      }}
                    </label>
                  </div>
                </div>
              </div>
            </div>
            <div v-if="checkFormality()">
              <DataTable :value="checkFormality().condition.filter((val) => {
                return val.status != 'D';
              })
                " showGridlines tableStyle="min-width: 50rem">
                <Column header="Tổng tiền hàng" headerStyle="min-width:10rem" v-if="
                  ['1-1', '1-2', '1-3', '1-4', '3-1', '3-2', '3-3'].includes(
                    dataEdit.promotionSubType
                  )
                ">
                  <template #body="slotProps">
                    <div class="flex align-items-center gap-2">
                      <span>Từ</span>
                      <InputNumber class="w-4" v-model="slotProps.data.amount"></InputNumber>
                    </div>
                  </template>
                </Column>
                <Column :header="getLabelItem(dataEdit.promotionSubType, 1)" headerStyle="min-width:30rem" v-if="
                  ['1-2', '1-3', '2-2', '2-1', '2-4', '3-1', '3-2', '3-3'].includes(
                    dataEdit.promotionSubType
                  )
                ">
                  <template #body="slotProps">
                    <div class="flex gap-1 align-items-start">
                      <InputNumber v-if="
                        ['1-2', '2-1', '1-3', '2-2', '3-1', '3-2', '3-3'].includes(
                          dataEdit.promotionSubType
                        )
                      " v-model="slotProps.data[setValueField(dataEdit.promotionSubType, 1)[0]]
                        " class="w-3"></InputNumber>
                      <div class="w-9">
                        <NodeGItem :data_req="slotProps.data[setValueField(dataEdit.promotionSubType, 1)[1]]
                          " @change="
                            slotProps.data[
                            setValueField(dataEdit.promotionSubType, 1)[1]
                            ] = SetData(
                              $event,
                              setValueField(dataEdit.promotionSubType, 1)[1],
                              slotProps.index
                            )
                            " />
                      </div>
                    </div>
                  </template>
                </Column>
                <Column header="Giá trị khuyến mại" headerStyle="min-width:20rem" v-if="
                  ['1-1', '1-3', '2-1', '3-1', '3-3'].includes(
                    dataEdit.promotionSubType
                  )
                ">
                  <template #body="slotProps">
                    <div class="flex align-items-center gap-2">
                      <InputNumber v-if="slotProps.data.typeDiscount == 1" class="w-3"
                        v-model="slotProps.data.discountAmount" :min="1" :max="slotProps.data.amount-1" ></InputNumber>
                      <InputNumber v-if="slotProps.data.typeDiscount != 1" :min="1" :max="100" class="w-3"
                        v-model="slotProps.data.discount"></InputNumber>
                      <div class="flex gap-2 w-9">
                        <Button label="VND" :severity="slotProps.data.typeDiscount == 1 ? 'primary' : 'secondary'
                          " @click="slotProps.data.typeDiscount = 1"/>
                        <Button label="%" :severity="slotProps.data.typeDiscount == 2 ? 'primary' : 'secondary'
                          " @click="slotProps.data.typeDiscount = 2"/>
                      </div>
                    </div>
                  </template>
                </Column>

                <Column :header="getLabelItem(dataEdit.promotionSubType, 2)" style="min-width: 30rem"
                  v-if="['2-1', '2-2', '3-2', '3-3'].includes(dataEdit.promotionSubType)">
                  <template #body="slotProps">
                    <div class="flex gap-1 align-items-start">
                      <InputNumber v-if="
                        ['2-1', '2-2', '3-2', '3-3'].includes(dataEdit.promotionSubType)
                      " v-model="slotProps.data[setValueField(dataEdit.promotionSubType, 2)[0]]
                        " class="w-3"></InputNumber>
                      <div class="w-9">
                        <NodeGItem :data_req="slotProps.data[setValueField(dataEdit.promotionSubType, 2)[1]]
                          " @change="
                            slotProps.data[
                            setValueField(dataEdit.promotionSubType, 2)[1]
                            ] = SetData(
                              $event,
                              setValueField(dataEdit.promotionSubType, 2)[1],
                              slotProps.index
                            )
                            " />
                      </div>
                    </div>
                  </template>
                </Column>
                <Column header="Tặng tổng" v-if="['1-4', '2-4'].includes(dataEdit.promotionSubType)">
                  <template #body="slotProps">
                    <div class="flex align-items-center gap-2">
                      <InputNumber class="w-6" v-model="slotProps.data.quantityAdd"></InputNumber>
                    </div>
                  </template>
                </Column>
                <Column style="min-width: 30rem" header="Voucher"
                  v-if="['1-4', '2-4'].includes(dataEdit.promotionSubType)">
                  <template #body="slotProps">
                    <AutoComplete class="w-full" placeholder="Chọn đợt phát hành"
                      v-model="slotProps.data.promotionVoucherLine" :suggestions="dataVoucher" @complete="FetchVoucher"
                      @change="
                        ChangeVoucher(
                          slotProps.data.promotionVoucherLine,
                          $event,
                          slotProps.index
                        )
                        " @item-unselec="Test()" optionLabel="voucherName" multiple />
                  </template>
                </Column>
                <Column v-if="dataEdit.promotionSubType == '2-3'">
                  <template #body="slotProps">
                    <div class="card">
                      <div class="grid">
                        <div class="col-2">
                          <h5>Khi mua</h5>
                        </div>
                        <div class="col-10">
                          <div class="mb-3">
                            <NodeGItem :data_req="slotProps.data.promotionLineItem" @change="
                              slotProps.data.promotionLineItem = SetData(
                                $event,
                                'promotionLineItem',
                                slotProps.index
                              )
                              " />
                          </div>
                          <div class="flex gap-3 mt-3 align-items-center mb-3" v-for="(
                              item, index
                            ) in slotProps.data.promotionDiscountLine.filter((val) => {
                                return val.status != 'D';
                              })" :key="index">
                            <h6 class="m-0">Số lượng</h6>
                            <InputNumber v-model="item.quantity"></InputNumber>
                            <Dropdown v-model="item.type" optionValue="code" optionLabel="name" :options="dataPrice"
                              class="w-2" />
                            <InputNumber v-model="item.price" v-if="item.type == 'GB'" />
                            <InputNumber v-if="item.type == 'GG' && item.typeDiscount == 1"
                              v-model="item.discountAmount"></InputNumber>
                            <InputNumber v-if="item.type == 'GG' && item.typeDiscount != 1" v-model="item.discount">
                            </InputNumber>
                            <div class="flex gap-2" v-if="item.type == 'GG'">
                              <Button label="VND" :severity="item.typeDiscount == 1 ? 'primary' : 'secondary'
                                " @click="item.typeDiscount = 1"/>
                              <Button label="%" :severity="item.typeDiscount == 2 ? 'primary' : 'secondary'
                                " @click="item.typeDiscount = 2"/>
                            </div>
                            <Button icon="pi pi-times" text severity="danger"
                              @click="RemoveSubCondition(item)"/>
                          </div>
                          <a class="mt-3" href="#" @click="AddSubCondition(slotProps.data.promotionDiscountLine)"><i
                              class="pi pi-plus mr-2"></i>Thêm dòng</a>
                        </div>
                      </div>
                    </div>
                  </template>
                </Column>
                <Column>
                  <template #body="slotProps">
                    <Button text icon="pi pi-trash" @click="removeConditon(slotProps.data)"/>
                  </template>
                </Column>
              </DataTable>
            </div>
          </div>
          <div class="flex align-items-center justify-content-start">
            <Button text icon="pi pi-plus-circle" label="Thêm điều kiện" @click="addCondition()"/>
          </div>
        </div>
      </TabPanel>
      <TabPanel header="Thời gian áp dụng">
        <div class="grid">
          <div class="col-6">
            <div class="flex flex-column gap-2">
              <div class="flex align-items-center gap-2 mb-2">
                <label class="font-semibold w-5 white-space-nowrap">Thời gian</label>
                <Calendar showIcon iconDisplay="input" class="w-full" v-model="dataEdit.fromDate" />
              </div>
            </div>
            <div class="flex align-items-center gap-2 mb-2">
              <label class="font-semibold w-5 white-space-nowrap">Đến</label>
              <Calendar showIcon iconDisplay="input" class="w-full" v-model="dataEdit.toDate" />
            </div>
            <div class="flex align-items-center gap-2 mb-2">
              <label class="font-semibold w-5 white-space-nowrap">Theo tháng</label>
              <MultiSelect class="w-full" display="chip" :options="[
                { name: 'Tháng 1', value: 1 },
                { name: 'Tháng 2', value: 2 },
                { name: 'Tháng 3', value: 3 },
                { name: 'Tháng 4', value: 4 },
                { name: 'Tháng 5', value: 5 },
                { name: 'Tháng 6', value: 6 },
                { name: 'Tháng 7', value: 7 },
                { name: 'Tháng 8', value: 8 },
                { name: 'Tháng 9', value: 9 },
                { name: 'Tháng 10', value: 10 },
                { name: 'Tháng 11', value: 11 },
                { name: 'Tháng 12', value: 12 },
              ]" optionLabel="name" optionValue="name" v-model="dataEdit.promotionMonths"></MultiSelect>
            </div>
            <div class="flex align-items-center gap-2 mb-2">
              <label class="font-semibold w-5 white-space-nowrap">Theo ngày</label>
              <MultiSelect class="w-full" :options="[
                { name: 'Ngày 1', value: 1 },
                { name: 'Ngày 2', value: 2 },
                { name: 'Ngày 3', value: 3 },
                { name: 'Ngày 4', value: 4 },
                { name: 'Ngày 5', value: 5 },
                { name: 'Ngày 6', value: 6 },
                { name: 'Ngày 7', value: 7 },
                { name: 'Ngày 8', value: 8 },
                { name: 'Ngày 9', value: 9 },
                { name: 'Ngày 10', value: 10 },
                { name: 'Ngày 11', value: 11 },
                { name: 'Ngày 12', value: 12 },
                { name: 'Ngày 13', value: 13 },
                { name: 'Ngày 14', value: 14 },
                { name: 'Ngày 15', value: 15 },
                { name: 'Ngày 16', value: 16 },
                { name: 'Ngày 17', value: 17 },
                { name: 'Ngày 18', value: 18 },
                { name: 'Ngày 19', value: 19 },
                { name: 'Ngày 20', value: 20 },
                { name: 'Ngày 21', value: 21 },
                { name: 'Ngày 22', value: 22 },
                { name: 'Ngày 23', value: 23 },
                { name: 'Ngày 24', value: 24 },
                { name: 'Ngày 25', value: 25 },
                { name: 'Ngày 26', value: 26 },
                { name: 'Ngày 27', value: 27 },
                { name: 'Ngày 28', value: 28 },
                { name: 'Ngày 29', value: 29 },
                { name: 'Ngày 30', value: 30 },
                { name: 'Ngày 31', value: 31 },
              ]" optionLabel="name" optionValue="name" display="chip" v-model="dataEdit.promotionDays"></MultiSelect>
            </div>
          </div>
          <div class="col-6">
            <div class="flex flex-column gap-2">
              <div class="flex align-items-center gap-2 mb-2">
                <label class="font-semibold w-5 white-space-nowrap">Theo thứ</label>
                <MultiSelect class="w-full" :options="[
                  { name: 'Thứ 2', value: 2 },
                  { name: 'Thứ 3', value: 3 },
                  { name: 'Thứ 4', value: 4 },
                  { name: 'Thứ 5', value: 5 },
                  { name: 'Thứ 6', value: 6 },
                  { name: 'Thứ 7', value: 7 },
                  { name: 'Chủ nhật ', value: 8 },
                ]" optionLabel="name" optionValue="name" display="chip" v-model="dataEdit.promotionweekdays">
                </MultiSelect>
              </div>
              <div class="flex align-items-center gap-2 mb-2">
                <label class="font-semibold w-5 white-space-nowrap">Theo giờ</label>
                <MultiSelect class="w-full" :options="[
                  { name: '0', value: 0 },
                  { name: '1', value: 1 },
                  { name: '2', value: 2 },
                  { name: '3', value: 3 },
                  { name: '4', value: 4 },
                  { name: '5', value: 5 },
                  { name: '6', value: 6 },
                  { name: '7', value: 7 },
                  { name: '8', value: 8 },
                  { name: '9', value: 9 },
                  { name: '10', value: 10 },
                  { name: '11', value: 11 },
                  { name: '12', value: 12 },
                  { name: '13', value: 13 },
                  { name: '14', value: 14 },
                  { name: '15', value: 15 },
                  { name: '16', value: 16 },
                  { name: '17', value: 17 },
                  { name: '18', value: 18 },
                  { name: '19', value: 19 },
                  { name: '20', value: 20 },
                  { name: '21', value: 21 },
                  { name: '22', value: 22 },
                  { name: '23', value: 23 },
                ]" display="chip" optionLabel="name" optionValue="name" v-model="dataEdit.promotionHours">
                </MultiSelect>
              </div>
              <div class="flex align-items-center gap-2 mb-2">
                <label class="font-semibold w-5">Tùy chọn khác</label>
                <div class="flex align-items-center gap-2 w-full">
                  <Checkbox v-model="dataEdit.notiAdded" binary trueValue="Y" falseValue="N"></Checkbox>
                  <span>Cảnh báo nếu khách hàng đã được hưởng khuyến mại này.</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </TabPanel>
      <TabPanel header="Phạm vi áp dụng">
        <div class="grid pt-2">
          <div class="col-6 flex flex-column gap-2">
            <div class="flex align-items-center gap-2">
              <RadioButton v-model="dataEdit.isAllSystem" value="Y"></RadioButton>
              <label class="w-5 font-semibold">Toàn hệ thống</label>
            </div>
            <div class="flex align-items-center gap-2">
              <RadioButton v-model="dataEdit.isAllSystem" value="N"></RadioButton>
              <label class="w-5 font-semibold">Chi nhánh</label>
              <MultiSelect :disabled="dataEdit.isAllSystem == 'Y'" v-model="dataEdit.promotionSystem" display="chip"
                :options="dataEdit.Branch" placeholder="Chọn chi nhánh áp dụng..." optionLabel="branchName"
                optionValue="id" class="w-full" />
            </div>
            <div class="flex align-items-center gap-2 mt-3">
              <RadioButton v-model="dataEdit.isAllSeller" value="Y"></RadioButton>
              <label class="w-5 font-semibold">Toàn bộ người bán</label>
            </div>
            <div class="flex align-items-center gap-2">
              <RadioButton v-model="dataEdit.isAllSeller" value="N"></RadioButton>
              <label class="w-5 font-semibold">Người bán</label>
              <Dropdown :disabled="dataEdit.isAllSeller == 'Y'" class="w-full" placeholder="Chọn người bán"
                v-model="dataEdit.promotionSeller"></Dropdown>
            </div>
          </div>
          <div class="col-6 flex flex-column gap-2">
            <div class="flex flex-column gap-2">
              <div class="flex align-items-center gap-2">
                <RadioButton v-model="dataEdit.isAllCustomer" value="Y"></RadioButton>
                <label class="w-5 font-semibold">Toàn bộ khách hàng</label>
              </div>
              <div class="flex align-items-center gap-2">
                <RadioButton v-model="dataEdit.isAllCustomer" value="N"></RadioButton>
                <label class="w-6 font-semibold">Nhóm khách hàng </label>
                <TreeSelect :disabled="dataEdit.isAllCustomer == 'Y'" selectionMode="checkbox"
                  v-model="dataEdit.promotionCustomer" :options="dataGroupCustomer" @change="ChangGroupCustomer($event)"
                  display="comma" placeholder="Chọn nhóm khách hàng" class="md:w-80 w-full" />
              </div>
            </div>
          </div>
        </div>
        <div>
          <Message severity="warn" :closable="false">Lưu ý: Chỉ thêm chương trình khuyến mại cho chi nhánh được phân
            quyền.</Message>
        </div>
      </TabPanel>
    </TabView>
    <template #footer>
      <div class="flex align-items-center gap-2 justify-content-end">
        <Button icon="pi pi-save" label="Lưu" @click="SavePromotion"/>
        <Button @click="releaseModal = false" class="bg-gray-500 text-white border-gray-500" icon="pi pi-times"
          label="Bỏ qua"/>
      </div>
    </template>
  </Dialog>

  <Dialog position="top" :draggable="false" v-model:visible="dialogDelete" :style="{ width: '500px' }" header="Xác Nhận"
    :modal="true">
    <div>
      <h6 v-if="dataEdit.delete" class="text-center">
        Bạn chắc chắn xoá khuyến mại đã chọn không?
      </h6>
    </div>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" outlined @click="dialogDelete = false" />
      <Button label="Xác nhận" icon="pi pi-check" @click="confirmReleaseCouponLine(dataEdit.delete)" />
    </template>
  </Dialog>
  <Loadding v-if="isLoading"></Loadding>
</template>
<script setup>
import { onMounted, ref, watchEffect } from "vue";
import API from '@/api/api-main'
import { format } from "date-fns";
import merge from "lodash/merge";
import { getCurrentInstance } from "vue";
import { useToast } from "primevue/usetoast";
import { useRouter, useRoute } from "vue-router";

const { proxy } = getCurrentInstance();
const toast = useToast();
const totalRecords = ref(0);
const pages = ref(0);
const rows = ref(10);
const router = useRouter();
const op = ref();
const API_URL = ref("promotions");
const releaseModal = ref(false);
const dialogDelete = ref(false);
const isLoading = ref(false);
const expandedRows = ref({});
const Products = ref([]);
const keySearchProduct = ref("");
const selectedColumn = ref([]);
const createDate = ref(true);
const dataSelected = ref({});
const dataPrice = ref([
  {
    code: "GB",
    name: "Giá bán",
  },
  {
    code: "GG",
    name: "Giảm giá",
  },
]);
const saleOffData = ref({
  total: "1",
  promo: [
    {
      id: 1,
      name: "Hoá đơn",
      formality: [
        {
          id: "1-1",
          name: "Giảm hoá đơn",
          condition: [
            {
              amount: 0,
              discount: 0,
              discountAmount: 0,
              typeDiscount: 1,
            },
          ],
        },
        {
          id: "1-2",
          name: "Tặng hàng",
          condition: [
            {
              amount: 0,
              quantityAdd: 0,
              promotionLineItem: [],
            },
          ],
        },
        {
          id: "1-3",
          name: "Giảm giá hàng",
          condition: [
            {
              amount: 0,
              discount: 0,
              discountAmount: 0,
              typeDiscount: 1,
              quantityAdd: 0,
              promotionLineItem: [],
            },
          ],
        },
        {
          id: "1-4",
          name: "Tặng voucher",
          condition: [
            {
              amount: 0,
              quantityAdd: 0,
              promotionVoucherLine: [],
            },
          ],
        },
      ],
    },
    {
      id: 2,
      name: "Hàng hoá",
      formality: [
        {
          id: "2-1",
          name: "Mua hàng giảm giá",
          isMulti: "Y",
          condition: [
            {
              quantity: 0,
              quantityAdd: 0,
              discount: 0,
              discountAmount: 0,
              typeDiscount: 1,
              promotionLineItem: [],
              promotionLineItemBonus: [],
            },
          ],
        },
        {
          id: "2-2",
          name: "Mua hàng tặng hàng",
          isMulti: "Y",
          condition: [
            {
              quantity: 0,
              promotionLineItem: [],
              quantityAdd: 0,
              promotionLineItemBonus: [],
            },
          ],
        },
        {
          id: "2-3",
          name: "Giá bán theo số lượng",
          condition: [
            {
              promotionLineItem: [],
              promotionDiscountLine: [
                {
                  quantity: 0,
                  type: "GB",
                  price: 0,
                  discount: 0,
                  discountAmount: 0,
                  typeDiscount: 1,
                },
              ],
            },
          ],
        },
        {
          id: "2-4",
          name: "Mua hàng tặng voucher",
          isMulti: "Y",
          condition: [
            {
              quantity: 0,
              promotionLineItem: [],
              quantityAdd: 0,
              promotionVoucherLine: [],
            },
          ],
        },
      ],
    },
    {
      id: 3,
      name: "Hoá đơn & Hàng hoá",
      formality: [
        {
          id: "3-1",
          name: "Giảm giá hoá đơn",
          condition: [
            {
              amount: 0,
              quantityAdd: 0,
              discount: 0,
              discountAmount: 0,
              typeDiscount: 1,
              promotionLineItem: [],
              typeDiscount: 1,
            },
          ],
        },
        {
          id: "3-2",
          name: "Tặng hàng",
          condition: [
            {
              amount: 0,
              quantity: 0,
              promotionLineItem: [],
              quantityAdd: 0,
              promotionLineItemBonus: [],
            },
          ],
        },
        {
          id: "3-3",
          name: "Giảm giá",
          condition: [
            {
              discount: 0,
              discountAmount: 0,
              typeDiscount: 1,
              amount: 0,
              quantity: 0,
              promotionLineItem: [],
              quantityAdd: 0,
              promotionLineItemBonus: [],
            },
          ],
        },
      ],
    },
  ],
});
const DataPromotion = ref([]);
const DataReset = JSON.stringify(saleOffData.value);
const defaultColumns = ref([
  {
    field: "promotionName",
    header: "Tên chương trình",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "text",
  },
  {
    field: "fromDate",
    header: "Từ ngày",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "time",
  },
  {
    field: "toDate",
    header: "Đến ngày",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "time",
  },
  {
    field: "",
    header: "Hình thức",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "doubleField",
    value: ["promotionType", "promotionSubType"],
  },
  {
    field: "price",
    header: "Người tạo",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "text",
  },
  {
    field: "promotionStatus",
    header: "Trạng thái",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "bool",
  },
]);
const column = ref([
  {
    field: "promotionName",
    header: "Mã chương trình",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "text",
  },
  {
    field: "fromDate",
    header: "Tên chương trình",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "time",
  },
  {
    field: "toDate",
    header: "Từ ngày",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "time",
  },
  {
    field: "rating",
    header: "Đến ngày",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "text",
  },
  {
    field: "",
    header: "Hình thức",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "doubleField",
    value: ["promotionType", "promotionSubType"],
  },
  {
    field: "price",
    header: "Người tạo",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "text",
  },
  {
    field: "price",
    header: "Ghi chú",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "text",
  },
  {
    field: "promotionStatus",
    header: "Trạng thái",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "bool",
  },
  {
    field: "price",
    header: "Hiệu lực",
    style: "text-align: start;",
    headerStyle: "min-width: 10rem;",
    type: "text",
  },
]);
let futureDate = new Date();
futureDate.setMonth(futureDate.getMonth() + 6);
const dataEdit = ref({
  promotionCode: "",
  promotionName: "",
  promotionDescription: "",
  promotionType: 1,
  promotionSubType: "1-1",
  fromDate: new Date(),
  toDate: futureDate,
  promotionMonths: "",
  promotionDays: "",
  promotionHours: "",
  promotionweekdays: "",
  notiAdded: "N",
  dob: "",
  isAllSystem: "Y",
  isAllCustomer: "Y",
  isAllSeller: "Y",
  promotionStatus: "Y",
  promotionLine: [],
  promotionSystem: [],
  promotionSeller: [],
  promotionCustomer: [],
});
const dataEditReset = JSON.stringify(dataEdit.value);
const dataGroupCustomer = ref([]);
const styleSticky = ref(null);
const filterPrromotion = ref({
  status: "All",
});
const submited = ref(false);
const dataVoucher = ref([]);
onMounted(() => {
  rows.value = useRoute().query.limit ? parseInt(useRoute().query.limit) : rows.value;
  pages.value = useRoute().query.skip ? parseInt(useRoute().query.skip) : pages.value;

  if (localStorage.getItem("selectedColumnSaleOff")) {
    selectedColumn.value = JSON.parse(localStorage.getItem("selectedColumnSaleOff"));
  } else {
    selectedColumn.value = defaultColumns.value;
  }
  //getGroupCustomer();
  fetchAllPromotion();
  window.addEventListener("scroll", handleScroll);
});
const rowClass = (data) => {
  if (dataSelected.value.check == false) return { "": dataSelected.value === data };

  return { "bg-primary": dataSelected.value === data };
};

watchEffect(() => {
  dataEdit.value.fromDate = new Date(dataEdit.value.fromDate);
  dataEdit.value.toDate = new Date(dataEdit.value.toDate);
});

const DetailPromotion = (event) => {
  if (expandedRows.value[event.data.id] != undefined) {
    expandedRows.value = {};
    dataSelected.value.check = false;
    rowClass(event.data);
  } else {
    dataSelected.value.check = true;
    expandedRows.value = [event.data].reduce((acc, p) => (acc[p.id] = true) && acc, {});
    dataSelected.value = event.data;
    rowClass(event.data);
  }
};
const fetchAllPromotion = async (q = null) => {
  try {
    let api_search ='';
  if(q!=null) api_search =`/search/${q}`;
    const res = await API.get(`${API_URL.value}${api_search}?page=${pages.value}&size=${rows.value}`);
    DataPromotion.value = res.data.promotions;
    totalRecords.value = res.data.total_count;
  } catch (err) {
    proxy.$notify("E", err, toast);
   
  } finally {
    isLoading.value = false;
    router.push(`?page=${pages.value}&size=${rows.value}`);
  }
};
const openReleaseDialog = async (data = null) => {
  if (data) {
    dataEdit.value.Status = "Fix";
    try {
      const res = await API.get(`${API_URL.value}/${data.id}`);
      const {
        promotionType,
        promotionSubType,
        promotionDays,
        promotionHours,
        promotionweekdays,
        promotionMonths,
      } = res.data;
      Object.assign(dataEdit.value, res.data, {
        promotionSubType: `${promotionType}-${promotionSubType}`,
        promotionDays: promotionDays ? promotionDays.split(",") : [],
        promotionHours: promotionHours ? promotionHours.split(",") : [],
        promotionweekdays: promotionweekdays ? promotionweekdays.split(",") : [],
        promotionMonths: promotionMonths ? promotionMonths.split(",") : [],
        promotionType: parseInt(promotionType),
      });

      dataEdit.value.promotionCustomerStoge = dataEdit.value.promotionCustomer;

      dataEdit.value.promotionCustomer = dataEdit.value.promotionCustomer.reduce(
        (acc, item) => {
          acc[item.groupId] = item;
          return acc;
        },
        {}
      );
      if (dataEdit.value.promotionSystem.length) {
        dataEdit.value.promotionSystemStoge = dataEdit.value.promotionSystem;
        dataEdit.value.promotionSystem = dataEdit.value.promotionSystem.map(
          (item) => item.systemId
        );
        dataEdit.value.promotionSystem = Object.values(dataEdit.value.promotionSystem);
      }

      const formality = checkFormality();
      formality.condition = merge([], formality.condition, dataEdit.value.promotionLine);
    } catch (error) {
      proxy.$notify("E", error, toast);
    }
  } else {
    ResetData();
  }
  // GetBranch();
  releaseModal.value = true;
};

const searchGlobal = () => {
  if (keySearchProduct.value === "") {
    fetchAllPromotion();
    return;
  }
  fetchAllPromotion(keySearchProduct.value);
};

const chooseColumn = () => {
  localStorage.setItem("selectedColumnSaleOff", JSON.ify(selectedColumn.value));
};
const checkCondition = () => {
  return saleOffData.value.promo.filter((val) => {
    return val.id == dataEdit.value.promotionType;
  })[0].formality;
};

const checkFormality = () => {
  return checkCondition().filter((val) => {
    return val.id == dataEdit.value.promotionSubType;
  })[0];
};

const getConditionData = (id) => {
  const conditions = {
    "1-1": { amount: 0, discount: 0, discountAmount: 0, typeDiscount: 1 },
    "1-2": { amount: 0, quantityAdd: 0, promotionLineItem: [] },
    "1-3": {
      amount: 0,
      discount: 0,
      discountAmount: 0,
      typeDiscount: 1,
      quantityAdd: 0,
      promotionLineItem: [],
    },
    "1-4": { amount: 0, quantityAdd: 0, promotionVoucherLine: [] },
    "2-1": {
      quantity: 0,
      quantityAdd: 0,
      discount: 0,
      discountAmount: 0,
      typeDiscount: 1,
      promotionLineItem: [],
      promotionLineItemBonus: [],
    },
    "2-2": {
      quantity: 0,
      promotionLineItem: [],
      quantityAdd: 0,
      promotionLineItemBonus: [],
    },
    "2-3": {
      promotionLineItem: [],
      promotionDiscountLine: [
        {
          quantity: 0,
          type: "GB",
          price: 0,
          discount: 0,
          discountAmount: 0,
          typeDiscount: 1,
        },
      ],
    },
    "2-4": {
      quantity: 0,
      promotionLineItem: [],
      quantityAdd: 0,
      promotionVoucherLine: [],
    },
    "3-1": {
      amount: 0,
      quantityAdd: 0,
      discount: 0,
      discountAmount: 0,
      typeDiscount: 1,
      promotionLineItem: [],
    },
    "3-2": {
      discount: 0,
      discountAmount: 0,
      typeDiscount: 1,
      amount: 0,
      quantity: 0,
      promotionLineItem: [],
      quantityAdd: 0,
      promotionLineItemBonus: [],
    },
    "3-3": {
      amount: 0,
      quantity: 0,
      promotionLineItem: [],
      quantityAdd: 0,
      promotionLineItemBonus: [],
    },
  };

  return conditions[id] || {};
};

const addCondition = () => {
  const formality = checkFormality();
  const conditionData = getConditionData(formality.id);

  if (Object.keys(conditionData).length) {
    conditionData.status = "A";
    formality.condition.push(conditionData);
  }
};
const removeConditon = (data) => {
  // checkFormality().condition = checkFormality().condition.filter((val) => {
  //   return val != data;
  // });
  data.status = "D";
};
const changePromo = () => {
  dataEdit.value.promotionSubType = checkCondition()[0].id;
};

const getLabelItem = (id, index) => {
  const labels = {
    "1-2_1": "Hàng/Nhóm hàng tặng",
    "1-3_1": "Hàng/Nhóm hàng được giảm giá",
    "2-1_1": "Hàng/Nhóm hàng mua",
    "2-1_2": "Hàng/Nhóm hàng được giảm giá",
    "2-2_1": "Hàng/Nhóm hàng mua",
    "2-2_2": "Hàng/Nhóm hàng tặng",
    "2-4_1": "Hàng/Nhóm hàng mua",
    "3-1_1": "Kèm hàng/nhóm hàng mua",
    "3-2_1": "Kèm hàng/nhóm hàng mua",
    "3-2_2": "Hàng/Nhóm hàng tặng",
    "3-3_1": "Kèm hàng/nhóm hàng mua",
    "3-3_2": "Hàng/Nhóm hàng được giảm giá",
  };
  return labels[`${id}_${index}`];
};

const setValueField = (id, index) => {
  const field = {
    "1-2_1": ["quantityAdd", "promotionLineItem"],
    "1-3_1": ["quantityAdd", "promotionLineItem"],
    "2-1_1": ["quantity", "promotionLineItem"],
    "2-1_2": ["quantityAdd", "promotionLineItemBonus"],
    "2-2_1": ["quantity", "promotionLineItem"],
    "2-2_2": ["quantityAdd", "promotionLineItemBonus"],
    "2-4_1": ["quantity", "promotionLineItem"],
    "3-1_1": ["quantityAdd", "promotionLineItem"],
    "3-2_1": ["quantity", "promotionLineItem"],
    "3-2_2": ["quantityAdd", "promotionLineItemBonus"],
    "3-3_1": ["quantity", "promotionLineItem"],
    "3-3_2": ["quantityAdd", "promotionLineItemBonus"],
  };
  return field[`${id}_${index}`];
};

const SavePromotion = async () => {
  submited.value = true;
  if (!Validates()) return;
  dataEdit.value.promotionLine = merge(
    [],
    dataEdit.value.promotionLine,
    checkFormality().condition
  );
  const fieldsToConvert = [
    "promotionDays",
    "promotionHours",
    "promotionweekdays",
    "promotionMonths",
    "promotionType",
  ];
  fieldsToConvert.forEach((field) => {
    dataEdit.value[field] = dataEdit.value[field].toString();
  });
  dataEdit.value.promotionSubType = dataEdit.value.promotionSubType.split("-")[1];
  const fieldsToEnsureArray = ["promotionSystem", "promotionSeller", "promotionCustomer"];
  fieldsToEnsureArray.forEach((field) => {
    dataEdit.value[field] = dataEdit.value[field] == null ? [] : dataEdit.value[field];
  });
  dataEdit.value.promotionCustomer = Object.values(dataEdit.value.promotionCustomer);

  dataEdit.value.promotionCustomer.forEach((item) => {
    item.promotionId = dataEdit.value.id ? dataEdit.value.id : 0;
    item.groupId = item.customerId;
    item.groupName = item.customerName;
    item.status = item.id ? "U" : "A";
  });

  if (dataEdit.value.promotionCustomerStoge) {
    dataEdit.value.promotionCustomerStoge.forEach((itemd) => {
      const indexToCheck = dataEdit.value.promotionCustomer.findIndex(
        (item) => item.groupId === itemd.groupId
      );
      itemd.status = "D";
      if (indexToCheck == -1) {
        dataEdit.value.promotionCustomer.push(itemd);
      }
    });
  }
  if (dataEdit.value.isAllSystem == "N") {
    const branchData = [];
    dataEdit.value.promotionSystem.forEach((item) => {
      const dataB = dataEdit.value.Branch.filter((val) => {
        return val.id == item;
      })[0];
      branchData.push({
        id: 0,
        promotionId: 0,
        systemId: dataB.id,
        systemCode: "",
        systemName: dataB.branchName,
        status: "A",
      });
    });
    if (branchData.length > 0) dataEdit.value.promotionSystem = branchData;
  }

  if (dataEdit.value.promotionSystemStoge) {
    dataEdit.value.promotionSystemStoge.forEach((item) => {
      const check = dataEdit.value.promotionSystem.some(
        (item1) => item1.systemId === item.systemId
      );
      if (!check) {
        item.status = "D";
        dataEdit.value.promotionSystem.push(item);
      } else {
        item.status = "U";
        const index = dataEdit.value.promotionSystem.findIndex(
          (item2) => item2.systemId === item.systemId
        );
        dataEdit.value.promotionSystem[index] = item;
      }
    });
  }
  releaseModal.value = false;
  isLoading.value = true;
  try {
    let FUNCAPI = dataEdit.value.id
      ? API.update(`${API_URL.value}/${dataEdit.value.id}`, dataEdit.value)
      : API.add(`${API_URL.value}/add`, dataEdit.value);
    const res = await FUNCAPI;
    if (res.data) proxy.$notify("S", "Thêm mới thành công !", toast);
  } catch (error) {
    proxy.$notify("E", error, toast);
  } finally {
    fetchAllPromotion();
    ResetData();
    isLoading.value = false;
    releaseModal.value = false;
    submited.value = false;
  }
};

const ConvertDataCustomer = (data) => {
  for (let index = 0; index < data.length; index++) {
    const item = data[index];
    item.key = item.id;
    item.label = item.groupName;
    if (item.child.length) {
      item.children = item.child;
      ConvertDataCustomer(item.child);
    }
  }
};

// const getGroupCustomer = async () => {
//   try {
//     const res = await API.get(`CustomerGroup`);
//     dataGroupCustomer.value = res.data.bpGroup;
//     ConvertDataCustomer(dataGroupCustomer.value);
//   } catch (error) {
//     proxy.$notify("E", error, toast);
//   }
// };

const ResetCondition = () => {
  saleOffData.value = { ...JSON.parse(DataReset) };
};

const ResetData = () => {
  dataEdit.value = { ...JSON.parse(dataEditReset) };
};

const SetData = (itemLine, name, index) => {
  const arrItemLine = itemLine.map((item) => ({
    itemId: item.type !== "G" ? 0 : item.id,
    itemCode: item.itemCode || "",
    itemName: item.name,
    itemGroupId: item.itemGroupId || 0,
    itmsGrpName: item.name,
    type: ["G", "IG"].includes(item.type) ? "IG" : "IT",
  }));

  const promotionLine = dataEdit.value.promotionLine[index]?.[name] || [];

  if (arrItemLine.length > 0 && dataEdit.value.Status === "Fix") {
    promotionLine.forEach((element) => {
      const exists = arrItemLine.some(
        (val) =>
          (val.type === "IG" && val.itemGroupId === element.itemGroupId) ||
          (val.type === "IT" && val.itemCode === element.itemCode)
      );
      if (!exists) element.status = "D";
    });
  } else if (promotionLine.length > 0) {
    const hasDeleted = promotionLine.some((val) => val.status === "D");

    if (promotionLine.length === 1 && hasDeleted) {
      promotionLine.forEach((element) => (element.status = "D"));
    }
  }

  return arrItemLine;
};

const AddSubCondition = (data) => {
  data.push({
    quantity: 0,
    type: "GB",
    price: 0,
    discount: 0,
    discountAmount: 0,
    typeDiscount: 1,
    status: "A",
  });
};
const RemoveSubCondition = (item) => {
  item.status = "D";
};

const getNameFormality = (id, subID) => {
  if (id == "" || subID == "") return "";
  const promotion = saleOffData.value.promo.filter((val) => {
    return val.id == id;
  })[0];

  if (promotion.formality.length > 0) {
    const promotionSub = promotion.formality.filter((val) => {
      return val.id == id + "-" + subID;
    });
    return promotion.name + " - " + promotionSub[0].name;
  }
};
const deleteItem = (data) => {
  dialogDelete.value = true;
  dataEdit.value.delete = { ...data };
};
const confirmReleaseCouponLine = async (data) => {
  try {
    const res = await API.delete(`${API_URL.value}/${data.id}`);
    if (res.data) proxy.$notify("S", "Đã xoá thành công", toast);
  } catch (error) {
    proxy.$notify("E", error, toast);
  } finally {
    fetchAllPromotion();
    dialogDelete.value = false;
  }
};

const handleScroll = () => {
  if (window.scrollY >= 109) {
    styleSticky.value = {
      position: "fixed",
      top: "0px",
      bottom: "auto",
      width: "82%",
      right: 0,
    };
  } else {
    styleSticky.value = {};
  }
};
const onCouponPageChange = (event) => {
  rows.value = event.rows;
  pages.value = event.page;
  fetchAllPromotion();
};
const ChangGroupCustomer = (event) => {
  for (const key in event) {
    const customer = FilterGCustomer(dataGroupCustomer.value, key);
    if (customer != null) {
      event[key].customerId = parseInt(key);
      event[key].customerName = customer.groupName;
    }
  }
};
const FilterGCustomer = (data, id) => {
  for (let i = 0; i < data.length; i++) {
    const item = data[i];
    if (item.id == id) {
      return item;
    }
    if (item.child && item.child.length > 0) {
      const result = FilterGCustomer(item.child, id);
      if (result) {
        return result;
      }
    }
  }
  return null;
};
const Validates = () => {
  if (!dataEdit.value.promotionName || dataEdit.value.promotionName == "") {
    proxy.$notify("E", "Vui lòng nhập tên chương trình khuyến mại", toast);
    return false;
  }
  if (checkFormality().condition.length) {
    const checkFormalityConditions = checkFormality().condition;
    for (let i = 0; i < checkFormalityConditions.length; i++) {
      const item = checkFormalityConditions[i];
      let status = true;
      if (item.amount == 0) status = false;
      if (item.discount == 0 && item.typeDiscount == 2) status = false;
      if (item.discountAmount == 0 && item.typeDiscount == 1) status = false;

      if (!status) {
        proxy.$notify("E", "Vui lòng nhập đủ thông tin điều kiện hình thức", toast);
        return false;
      }
    }
  }

  return true;
};

const FetchVoucher = async (q) => {
  try {
    const res = await API.get("Voucher?skip=0&limit=30");
    dataVoucher.value = res.data.items;
  } catch (error) { }
};

const ChangeVoucher = (data, event, index) => {
  const checkIiset = data.filter((val) => {
    return val.voucherCode == event.value[event.value.length - 1].voucherCode;
  });
  if (checkIiset.length > 1) {
    data = data.pop();
  }
  // Cập nhật trạng thái của các phần tử trong `event.value`
  event.value.forEach((item) => {
    if (!item.voucherId) {
      item.voucherId = item.id;
      item.id = 0;
      item.status = "A";
    } else if (item.status !== "A") {
      item.status = "U";
    }
  });

  // Tạo một Set chứa voucherId từ `event.value` để kiểm tra sự tồn tại nhanh hơn
  const voucherIdSet = new Set(event.value.map((item) => item.voucherId));
  // Cập nhật trạng thái của các phần tử trong `promotionVoucherLine`
  if (dataEdit.value.promotionLine.length) {
    const promotionVoucherLine = dataEdit.value.promotionLine[index].promotionVoucherLine;
    promotionVoucherLine.forEach((element) => {
      if (!voucherIdSet.has(element.voucherId)) {
        element.status = "D";
      }
    });
  }

  // Nếu `event.value` không có phần tử nào, đánh dấu tất cả các phần tử trong `promotionVoucherLine` là "D"
  if (event.value.length === 0) {
    promotionVoucherLine.forEach((element) => (element.status = "D"));
  }
};
const GetBranch = async () => {
  try {
    const res = await API.get("Branch?skip=0&limit=30");
    dataEdit.value.Branch = res.data.item;

  } catch (error) {
    proxy.$notify("E", error, toast);
  }
};
</script>
<style>
.p-inputnumber>input {
  width: 100%;
}

.p-autocomplete-input {
  width: 100%;
}

.p-autocomplete-multiple-container {
  width: 100%;
}
</style>
