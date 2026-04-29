<template>
    <div class="flex justify-content-between align-items-center mb-4">
        <div class="uppercase font-bold">Danh mục cơ cấu tổ chức</div>
        <Button label="Thêm mới" icon="fa-solid fa-plus" class="py-2 text-sm"/>
    </div>

    <div class="grid nested-grid">
        <div class="col-3">
            <div class="card h-screen bg-white">
                <IconField iconPosition="left">
                    <InputIcon class="pi pi-search"> </InputIcon>
                    <InputText placeholder="Tìm kiếm" class="w-full" />
                </IconField>
            </div>
        </div>
        <div class="col-9">
            <div class="col-12 card">
                <div class="uppercase font-bold">Công ty tnhh mtv dầu khí thành phố hồ chí minh</div>
                <div class="flex mt-3">
                    <p class="mb-0">Mã đơn vị: SGP</p>
                    <p class="px-2 mb-0">|</p>
                    <p class="mb-0">Cấp tổ chức: Tập đoàn</p>
                </div>
            </div>
            <div class="col-12 card">
                <div class="h-30rem">a</div>
            </div>
        </div>
    </div>
</template>
<style scoped></style>
