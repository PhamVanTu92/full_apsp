<template>
  <div class="surface-200 relative h-full">
    <ul class="m-0 menu ">
      <li :class="{'item-menu': item.items }" class="hover:surface-300"
        v-for="(item, i) in menuItems" :key="i">
        <a v-if="item.link" :href="item.link" class="flex gap-3 p-3">
          <Image v-if="item.icon" :src="item.icon" alt="Image" width="40" />
          <div class="flex flex-grow-1 align-items-center justify-content-between">
            <span class="select-none font-bold uppercase selection-none">{{ item.label }}</span>
            <i v-if="item.items.length > 0" class="fa-solid fa-chevron-right mr-2"></i>
          </div>
        </a>
        <div v-else class="flex gap-3 p-3">
          <Image v-if="item.icon" :src="item.icon" alt="Image" width="40" />
          <div class="flex flex-grow-1 align-items-center justify-content-between">
            <span class="select-none font-bold uppercase selection-none">{{ item.label }}</span>
            <i v-if="item.items.length > 0" class="fa-solid fa-chevron-right mr-2"></i>
          </div>
        </div>
        <div v-if="item.items.length > 0" class="right-category hidden overflow-y-scroll p-5 cus-shadown">
          <div class="grid">
              <ul v-for="(subItem, j) in item.items" :key="j" class="sub-content col-4">
                <a class="font-bold py-2" v-if="subItem.link" :href="subItem.link">{{ subItem.label }}</a>
                <span v-else class="font-bold py-2">{{ subItem.label }}</span>
                <li :class="{'mt-2':k == 0}" v-for="(_el, k) in subItem.items" :key="k" class="m-0 py-2">
                  <a v-if="_el.link" :href="_el.link">{{ _el.label }}</a>
                  <span v-else >{{ _el.label }}</span>
                </li>
              </ul>
            </div>
        </div>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';

onMounted(() => {
  // rightCategory.value = menuItems[0];
});

var menuItems = [
  {
    label: "SAIGON PETRO",
    icon: "/image/category_icons/0.png",
    link: "#",
    items: [
      {
        label: "Dầu Nhớt Xe Số, Xe Tay Ga",
        icon: "",
        link: "#",
        items: [
          {
            label: "Dầu Nhớt Xe Số",
            icon: "",
            link: "#",
            items: []
          },
          {
            label: "Xe Tay Ga",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ 2 Kỳ",
            icon: "",
            link: "",
            items: []
          }
        ]
      },
      {
        label: "Dầu Nhớt Xe Ôtô Và Xe Bán Tải",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Cầu, Dầu Hộp Số",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ Xăng",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ Diesel",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Phụ Trợ",
            icon: "",
            link: "",
            items: []
          }
        ]
      },
      {
        label: "Dầu Động Cơ Xe Tải Nặng & Hàng Hải",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ 2 Kỳ Thấp Tốc",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ 2 Kỳ Trung Tốc",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ Turbo",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ Thông Dụng",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Cầu, Dầu Hộp Số",
            icon: "",
            link: "",
            items: []
          }
        ]
      },
      {
        label: "Dầu Thuỷ Lực",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Thuỷ Lực Không Kẽm",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Thuỷ Lực Gốc Kẽm",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Thuỷ Lực HVI",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Thuỷ Lực Chống Cháy",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Truyền Động Đa Na",
            icon: "",
            link: "",
            items: []
          }
        ]
      },
      {
        label: "Dầu Nông Nghiệp",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ Máy Nông Nghiệp",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Truyền Động Máy Nông Nghiệp",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Bôi Trơn Đa Dụng",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Bôi Trơn Đa Chức Năng",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Mỡ Bôi Trơn",
        icon: "",
        link: "",
        items: [
          {
            label: "Mỡ Bôi Trơn Đa Năng",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Nước Làm Mát",
        icon: "",
        link: "",
        items: [
          {
            label: "Nước Làm Mát Động Cơ",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Công Nghiệp",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Bánh Răng Công Nghiệp",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Turbin",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Chống Rỉ",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Truyền Nhiệt",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Tuần Hoàn",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Máy Nén Lạnh",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Máy Nén Khí",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Đường Trượt",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Cắt Gọt Pha Nước",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Bơm Chân Không",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Máy Dệt",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Tháo Khuôn",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Biến Thế",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Trắng Kỹ Thuật",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Bôi Trơn Đa Năng",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Xích Tải",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Trục Quay, Trục Chính",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Tôi Kim Loại",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
    ]
  },
  {
    label: "AP OIL",
    icon: "/image/category_icons/1.png",
    link: "#",
    items: [
      {
        label: "Dầu Nhớt Xe Số, Xe Tay Ga",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Nhớt Xe Số",
            icon: "",
            link: "#",
            items: []
          },
          {
            label: "Xe Tay Ga",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ 2 Kỳ",
            icon: "",
            link: "",
            items: []
          }
        ]
      },
      {
        label: "Dầu Nhớt Xe Ôtô Và Xe Bán Tải",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ Xăng",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ Diesel",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Phụ Trợ",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Cầu, Dầu Hộp Số",
            icon: "",
            link: "",
            items: []
          }
        ]
      },
      {
        label: "Dầu Động Cơ Xe Tải Nặng & Hàng Hải",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ Thông Dụng",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ Turbo",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Cầu, Dầu Hộp Số",
            icon: "",
            link: "",
            items: []
          }
        ]
      },
      {
        label: "Dầu Thuỷ Lực",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Thuỷ Lực Không Kẽm",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Thuỷ Lực Gốc Kẽm",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Thuỷ Lực HVI",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Nông Nghiệp",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ Máy Nông Nghiệp",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Truyền Động Máy Nông Nghiệp",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Mỡ Nhờn",
        icon: "",
        link: "",
        items: [
          {
            label: "Mỡ Đa Dụng Chịu Cực Áp",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Mỡ Phức Lithium chịu nhiệt cao",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Mỡ Lithium Đa Năng",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Mỡ Siêu Chịu Nhiệt",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Mỡ Cáp Hàng Hải",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Công Nghiệp",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Bánh Răng Công Nghiệp",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Turbin",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Truyền Nhiệt",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Máy Nén Lạnh",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Tôi Kim Loại",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Tuần Hoàn",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Xúc Rửa Và Tẩy Rửa",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Máy Nén Khí",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Máy Bơm Chân Không",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Rãnh Trượt",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Cắt Gọt Pha Nước",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Bơm Chân Không",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Máy Dệt May",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Trắng Kỹ Thuật",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Trục Quay, Trục Chính",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
    ]
  },
  {
    label: "SINO",
    icon: "/image/category_icons/2.png",
    link: "",
    items: [
      {
        label: "Dầu Hộp Số",
        icon: "",
        link: "",
        items: []
      },
      {
        label: "Dầu Thuỷ Lực",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Thuỷ Lực Gốc Kẽm",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Nhớt Xe Số, Xe Tay Ga",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ Xe Số",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ Xe Tay Ga",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Nhớt Xe Ôtô Và Bán Tải",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ Xăng",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ Diesel",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Phụ Trợ",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Động Cơ Xe Tải Và Thiết Bị Xe Cơ Giới",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ Turbo",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ Thông Dụng",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
    ]
  },
  {
    label: "SINGTEC OIL",
    icon: "/image/category_icons/3.png",
    link: "",
    items: [
      {
        label: "Dầu Công Nghiệp",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Bánh Răng, Hộp Số",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Động Cơ Xe Tải Nặng Và Hàng Hải",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ Turbo",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Thuỷ Lực",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Thuỷ Lực Gốc Kẽm",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
    ]
  },
  {
    label: "POLARIS",
    icon: "/image/category_icons/4.png",
    link: "",
    items: []
  },
  {
    label: "EXPORT & TOLL BLENDING",
    icon: "/image/category_icons/5.png",
    link: "",
    items: [
      {
        label: "Dầu Nông Nghiệp",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Truyền Động Máy Nông Nghiệp",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
      {
        label: "Dầu Động Cơ",
        icon: "",
        link: "",
        items: [
          {
            label: "Dầu Động Cơ Xăng 2 Kỳ",
            icon: "",
            link: "",
            items: []
          },
          {
            label: "Dầu Động Cơ 4 Kỳ",
            icon: "",
            link: "",
            items: []
          },
        ]
      },
    ]
  },
]

</script>

<style scoped>

.cus-shadown{
  box-shadow: rgba(0, 0, 0, 0.055) 2px 2px 6px 1px inset, rgba(0, 0, 0, 0.055) -1px -1px 6px 1px inset;
}

a{
  color: var(--text-color);
}

a:hover{
  font-weight: 600;
  color: var(--primary-500);
}

.sub-content{
  list-style: none;
}


.right-category {
  position: absolute;
  right: -72.5rem;
  top: 0;
  background: var(--surface-100);
  width: 72.5rem;
  height: 100%;
  z-index: 1000;
}

.hidden {
  display: none;
}

.item-menu:hover .hidden{
  display: block !important;
}

.menu {
  list-style: none;
  padding: 0;
}

/* width */
.right-category::-webkit-scrollbar {
  width: 10px;
}

/* Track */
.right-category::-webkit-scrollbar-track {
  background: var(--surface-200);
}

/* Handle */
.right-category::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 3px;
}

/* Handle on hover */
.right-category::-webkit-scrollbar-thumb:hover {
  background: #555;
}
</style>
