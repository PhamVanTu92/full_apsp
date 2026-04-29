import { createRouter, createWebHistory } from 'vue-router';
import AppLayout from '@/layout/AppLayout.vue';
import Default from '../views/client/layout/default.vue';
import auth from '../middlewares/auth.middleware';
import authourization from '../middlewares/authourization.middleware';
import GuessLayout from '@/views/client/layout_nologin/GuessLayout.vue';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            component: AppLayout,
            meta: {
                middleware: [auth, authourization]
            },
            children: [
                {
                    path: '/',
                    name: 'dashboard',
                    component: () => import('@/views/admin/Dashboard/Dashboard.vue')
                },
                //Report
                { path: 'report', name: 'report', component: () => import('@/views/admin/Report/index.vue') },
                {
                    path: 'report',
                    name: 'Report',
                    children: [
                        {
                            path: 'buy-by-product',
                            name: 'buyByProduct',
                            component: () => import('@/views/admin/Report/buyByProduct.vue')
                        },
                        {
                            path: 'accounts-payable',
                            name: 'accountsPayable',
                            component: () => import('@/views/admin/Report/AccountsPayableReport/accountsPayableReport.vue')
                        },
                        {
                            path: 'inventory-send',
                            name: 'inventorySend',
                            component: () => import('@/views/admin/Report/goodInventorySend.vue')
                        },
                        {
                            path: 'inventory-report',
                            name: 'inventoryReport',
                            component: () => import('@/views/admin/Report/inventoryReport.vue')
                        },
                        {
                            path: 'average-price',
                            name: 'averagePrice',
                            component: () => import('@/views/admin/Report/averagePriceReport.vue')
                        },
                        {
                            path: 'return-goods',
                            name: 'returnGoods',
                            component: () => import('@/views/admin/Report/returnGoodsReport.vue')
                        },
                        {
                            path: 'consent-log',
                            name: 'consent-log',
                            component: () => import('@/views/admin/Report/NhatKyHeThong/index.vue')
                        },
                        {
                            path: 'bonus-point',
                            name: 'bonus-point',
                            component: () => import('@/views/admin/Report/BonusPoint/index.vue')
                        },
                        {
                            path: 'report-zalo',
                            name: 'report-zalo',
                            component: () => import('@/views/admin/Report/ReportZalo/index.vue')
                        },
                        {
                            path: 'system-log',
                            name: 'system-log',
                            component: () => import('@/views/admin/Report/NhatKyHoatDong/index.vue')
                        },
                        {
                            path: 'commited-report',
                            name: 'commitedReport',
                            component: () => import('@/views/admin/Report/committedOutputReport.vue')
                        },
                        {
                            path: 'purchase-report-by-order',
                            name: 'purchase-report-by-order',
                            component: () => import('@/views/admin/Report/PurchaseReportByOrder.vue')
                        },
                        {
                            path: 'purchase-plan-report',
                            name: 'purchase-plan-report',
                            component: () => import('@/views/admin/Report/purchasePlanReport.vue')
                        },
                        {
                            path: 'invoice-statistics-report',
                            name: 'invoice-statistics-report',
                            component: () => import('@/views/admin/Report/InvoiceStatisticsReport/index.vue')
                        },
                        {
                            path: 'debt-ledger',
                            name: 'debt-ledger',
                            component: () => import('@/views/admin/Report/DebtLedger/index.vue')
                        },
                        {
                            path: 'payment',
                            name: 'payment',
                            component: () => import('../views/admin/Report/ThanhToanNgay/index.vue')
                        },
                        {
                            path: 'committed',
                            name: 'committed',
                            component: () => import('../views/admin/Report/CamKetSanLuong/index.vue')
                        }
                    ]
                },

                //End report
                {
                    path: 'user-management',
                    name: 'Usermanagement',
                    component: () => import('@/views/admin/User/UserList.vue')
                },
                {
                    path: 'user-group',
                    children: [
                        {
                            path: 'list',
                            name: 'Usergroup',
                            component: () => import('@/views/admin/Group/Usergroup.vue')
                        }
                    ]
                },
                {
                    path: 'roles',
                    name: 'roles',
                    component: () => import('../../src/views/admin/Role/index.vue')
                },
                {
                    path: 'terms',
                    name: 'terms',
                    component: () => import('../../src/views/admin/SystemConfig/Terms/index.vue')
                },
                {
                    path: 'settings',
                    name: 'settings',
                    component: () => import('../../src/views/admin/Settings/General/index.vue')
                },

                {
                    path: 'purchase-forcast/forcast/:id',
                    name: 'DetailOrderPlaning',
                    component: () => import('@/views/admin/OrderPlanning/DetailOrderPlan.vue')
                },
                {
                    path: 'agen-man',
                    component: () => import('@/views/admin/MasterData/AgenMan/index.vue'),
                    children: [
                        {
                            path: 'agency-category',
                            name: 'agencyCategory',
                            component: () => import('@/views/admin/MasterData/AgenMan/AgencyCategory.vue')
                        },
                        {
                            path: 'agency-category/:id',
                            name: 'agencyCategory-detail',
                            component: () => import('@/views/admin/MasterData/AgenMan/views/AgencyDetail.vue')
                        },
                        {
                            path: 'agency-grp',
                            name: 'agencyGroup',
                            component: () => import('@/views/admin/UserGroup/index.vue')
                        },
                        {
                            path: 'agency-category/create',
                            name: 'agency-create',
                            component: () => import('@/views/admin/MasterData/AgenMan/views/AgencyCategory_Create.vue')
                        }
                    ]
                },
                {
                    path: 'material-goods',
                    component: () => import('@/views/admin/MasterData/SupplyItems/index.vue'),
                    children: [
                        {
                            path: 'itm-type',
                            name: 'itemType',
                            component: () => import('@/views/admin/MasterData/SupplyItems/ItemType.vue')
                        },
                        {
                            path: 'list-product',
                            name: 'ListProduct',
                            component: () => import('@/views/admin/MasterData/SupplyItems/ProductData.vue')
                        },

                        {
                            path: 'group-product',
                            name: 'GroupProduct',
                            component: () => import('@/views/admin/MasterData/SupplyItems/GroupProduct.vue')
                        },
                        {
                            path: 'product-group',
                            name: 'product-group',
                            component: () => import('../views/admin/ProductGroup/index.vue')
                        },
                        {
                            path: 'uom',
                            name: 'unitOfMeasurement',
                            component: () => import('@/views/admin/MasterData/SupplyItems/Uom.vue')
                        },
                        {
                            path: 'uom-grp',
                            name: 'uomGroup',
                            component: () => import('@/views/admin/MasterData/SupplyItems/UomGroup.vue')
                        }
                    ]
                },
                {
                    path: 'list-product-retail',
                    children: [
                        {
                            path: '',
                            name: 'ListProductRetail',
                            component: () => import('@/views/admin/ProductRetail/ProductDataRetail.vue')
                        },
                        {
                            path: 'new',
                            name: 'new-retail',
                            component: () => import('@/views/admin/ProductRetail/ProductDataRetailEdit.vue')
                        },
                        {
                            path: 'detail/:id',
                            name: 'retail-detail',
                            component: () => import('@/views/admin/ProductRetail/ProductDataRetailEdit.vue')
                        }
                    ]
                },
                {
                    path: 'approval-setup',
                    component: () => import('@/views/admin/MasterData/ApprovalSetup/index.vue'),
                    children: [
                        {
                            path: 'level',
                            name: 'approvalLevel',
                            component: () => import('@/views/admin/MasterData/ApprovalSetup/ApprovalLevel.vue')
                        },
                        {
                            path: 'template',
                            name: 'approvalTemplate',
                            component: () => import('@/views/admin/MasterData/ApprovalSetup/ApprovalTemplate.vue')
                        },
                        {
                            path: 'order-approval/:id?',
                            name: 'orderApproval',
                            component: () => import('@/views/admin/MasterData/ApprovalSetup/OrderApproval.vue')
                        },
                        {
                            path: 'status-report',
                            name: 'approvalStatusReport',
                            component: () => import('@/views/admin/MasterData/ApprovalSetup/StatusReport.vue')
                        }
                    ]
                },

                {
                    path: 'request-sample',
                    name: 'requestSample',
                    component: () => import('@/views/admin/RequestSample/index.vue')
                },
                {
                    path: 'request-sample/:id',
                    name: 'request-sameple-edit',
                    component: () => import('@/views/admin/RequestSample/detail.vue')
                },
                {
                    path: 'request-sample/create',
                    name: 'request-sample-create',
                    component: () => import('@/views/admin/RequestSample/create.vue')
                },
                {
                    path: 'promotion',
                    name: 'promotion',
                    children: [
                        {
                            path: '',
                            name: 'promotion-data',
                            component: () => import('@/views/admin/Promotion/PromotionData.vue')
                        },
                        {
                            path: 'new',
                            name: 'new-promotion_1',
                            component: () => import('@/views/admin/Promotion/PromotionEdit.vue')
                        },
                        {
                            path: 'detail/:id',
                            name: 'new_promotion_detail',
                            component: () => import('@/views/admin/Promotion/PromotionEdit.vue')
                        }
                    ]
                },
                {
                    path: 'predeempromotionals',
                    name: 'Predeempromotionals',
                    children: [
                        {
                            path: '',
                            name: 'predeempromotionals-data',
                            component: () => import('@/views/admin/RedeemPromotional/RedeemPromotionalData.vue')
                        },
                        {
                            path: 'new',
                            name: 'new-predeempromotionals',
                            component: () => import('@/views/admin/RedeemPromotional/RedeemPromotionalEdit.vue')
                        },
                        {
                            path: 'detail/:id',
                            name: 'detail-predeempromotionals',
                            component: () => import('@/views/admin/RedeemPromotional/RedeemPromotionalEdit.vue')
                        }
                    ]
                },
                {
                    path: 'promotionalPoints',
                    name: 'PromotionalPoints',
                    children: [
                        {
                            path: '',
                            name: 'promotionalPoints',
                            component: () => import('@/views/admin/RedeemPromotionalPoint/RedeemPromotionalData.vue')
                        },
                        {
                            path: 'new',
                            name: 'new-promotionalPoints',
                            component: () => import('@/views/admin/RedeemPromotionalPoint/RedeemPromotionalEdit.vue')
                        },
                        {
                            path: 'detail/:id',
                            name: 'detail-promotionalPoints',
                            component: () => import('@/views/admin/RedeemPromotionalPoint/RedeemPromotionalEdit.vue')
                        }
                    ]
                },

                {
                    path: 'decentralization',
                    name: 'Decentralization',
                    children: [
                        {
                            path: '',
                            name: 'Decentralization-user',
                            component: () => import('@/views/admin/Decentralization/index.vue')
                        },
                        {
                            path: 'role',
                            name: 'Decentralization-role',
                            component: () => import('@/views/admin/Decentralization/edit.vue')
                        }
                    ]
                },
                {
                    path: 'feedback-admin',
                    name: 'feedback-admin',
                    component: () => import('../views/admin/Feedback/index.vue')
                },
                {
                    path: 'create-customer',
                    name: 'add-customer',
                    component: () => import('../views/admin/Decentralization/CreateNPP.vue')
                },
                {
                    path: 'voucher',
                    name: 'voucher',
                    component: () => import('@/views/admin/Voucher/voucher.vue')
                },
                {
                    path: 'Coupon',
                    name: 'Coupon',
                    component: () => import('@/views/admin/Coupon/Coupon.vue')
                },
                {
                    path: 'Zalo',
                    name: 'Zalo',
                    component: () => import('@/views/admin/Report/ReportZalo/list.vue')
                },
                {
                    path: 'purchase-request',
                    children: [
                        {
                            path: 'list',
                            name: 'PurchaseRequest',
                            component: () => import('@/views/admin/PurchaseRequest/PurchaseRequest.vue')
                        },
                        {
                            path: 'detail/:id',
                            name: 'DetailPurchaseRequest',
                            component: () => import('@/views/admin/PurchaseRequest/Detail.vue')
                        }
                    ]
                },
                {
                    path: 'purchase-request/:id',
                    name: 'purchase-request-detail',
                    component: () => import('../views/admin/PurchaseRequest/PRDetail.vue')
                },
                {
                    path: 'promotional-request',
                    children: [
                        {
                            path: 'add',
                            name: 'change_point_add',
                            component: () => import('@/views/admin/RequestGift/Promotional.vue')
                        },
                        {
                            path: ':id',
                            name: 'change_point_edit',
                            component: () => import('@/views/common/Order/indexDoiVPKM.vue')
                        },
                        {
                            path: '',
                            name: 'promotiondetail',
                            component: () => import('@/views/admin/RequestPromotional/PromotionDetail.vue')
                        }
                    ]
                },
                {
                    path: 'gift-request',
                    children: [
                        {
                            path: '',
                            name: 'giftlist',
                            component: () => import('@/views/admin/RequestGift/Gift.vue')
                        },
                        {
                            path: 'detail',
                            name: 'giftdetail',
                            component: () => import('@/views/admin/RequestGift/GiftDetail.vue')
                        },
                        {
                            path: 'detail/:id',
                            name: 'giftCopy',
                            component: () => import('@/views/admin/RequestGift/GiftDetail.vue')
                        }
                    ]
                },
                {
                    path: 'order-net',
                    children: [
                        {
                            path: '',
                            name: 'orderNetList',
                            component: () => import('@/views/admin/OrderNET/TableOrder.vue')
                        },
                        {
                            path: 'detail',
                            name: 'orderNetDetail',
                            component: () => import('@/views/admin/OrderNET/OrderNETDetail.vue')
                        },
                        {
                            path: 'copy/:id',
                            name: 'orderNetCopy',
                            component: () => import('@/views/admin/OrderNET/OrderNETCopy.vue')
                        }
                    ]
                },
                {
                    path: 'return-request',
                    children: [
                        {
                            path: '',
                            name: 'returnRequestList',
                            component: () => import('@/views/admin/ReturnRequest/ReturnRequestList.vue')
                        },
                        {
                            path: 'create-independent',
                            name: 'returnRequestIndependent',
                            component: () => import('@/views/admin/ReturnRequest/ReturnRequestIndependent.vue')
                        },
                        {
                            path: 'select-order',
                            name: 'returnRequestSelectOrder',
                            component: () => import('@/views/admin/ReturnRequest/ReturnRequestSelectOrder.vue')
                        },
                        {
                            path: 'from-order/:id',
                            name: 'returnRequestFromOrder',
                            component: () => import('@/views/admin/ReturnRequest/ReturnRequestFromOrder.vue')
                        },
                        {
                            path: 'detail/:id',
                            name: 'returnRequestDetail',
                            component: () => import('@/views/common/ReturnRequest/ReturnRequestDetail.vue'),
                            props: { isClient: false }
                        }
                    ]
                },
                {
                    path: 'order-planning',
                    children: [
                        {
                            path: 'list',
                            name: 'orderPlanning',
                            component: () => import('@/views/admin/OrderPlanning/orderPlanning.vue')
                        },
                        {
                            path: 'detail/:id',
                            name: 'orderPlanningDetail',
                            component: () => import('@/views/admin/OrderPlanning/DetailOrderPlan.vue')
                        }
                    ]
                },
                {
                    path: 'warehouse-storage-fee',
                    children: [
                        {
                            path: 'list',
                            name: 'warehouse-storage-fee',
                            component: () => import('@/views/admin/Consignment_Fee/WarehouseStorageFee.vue')
                        }
                    ]
                },
                {
                    path: 'storage-fee-pricing',
                    name: 'storage-fee-pricing',
                    children: [
                        {
                            path: 'list',
                            name: 'storage-fee-pricing-list',
                            component: () => import('@/views/admin/Consignment_Fee/StorageFeePricing.vue')
                        },
                        {
                            path: 'new',
                            name: 'AddNewWarehouseFee',
                            component: () => import('@/views/admin/Consignment_Fee/components/AddNewWarehouseFee.vue')
                        },
                        {
                            path: 'update/:id',
                            name: 'UpdateWarehouseFee',
                            component: () => import('@/views/admin/Consignment_Fee/components/AddNewWarehouseFee.vue')
                        }
                    ]
                },
                {
                    path: 'purchase-order',
                    children: [
                        {
                            path: '',
                            name: 'purchaseList',
                            component: () => import('@/views/admin/PurchaseOrder/OrderList.vue')
                        },
                        // {
                        //     path: 'purchase-detail/:id',
                        //     name: 'DetailOrder',
                        //     component: () => import('@/views/admin/PurchaseOrder/DetailOrder.vue')
                        // },
                        {
                            path: ':id',
                            name: 'order-detail',
                            component: () => import('@/views/common/Order/index.vue')
                        },
                        {
                            path: 'new',
                            name: 'NewOrder',
                            component: () => import('@/views/admin/PurchaseOrder/NewOrder.vue')
                        },
                        {
                            path: 'purchase-order-copy/:id',
                            name: 'purchase-order-copy-admin',
                            component: () => import('../views/common/PurchaseOrder/indexCopyOrder.vue')
                        },
                    ]
                },
                {
                    path: 'purchase-order-new',
                    name: 'purchase-order-new',
                    component: () => import('../views/common/PurchaseOrder/index.vue')
                },
                {
                    path: 'purchase-order-new-plus',
                    name: 'purchase-order-new-plus',
                    component: () => import('../views/common/PurchaseOrder copy/index.vue')
                },
                {
                    path: 'commit/commited-outputing',
                    name: 'commited-outputing',
                    component: () => import('@/views/admin/CommittedOutput/index.vue')
                },
                {
                    path: 'profile',
                    name: 'profile',
                    component: () => import('@/views/admin/Profile/index.vue')
                },
                {
                    path: '/notifications',
                    name: 'notifications',
                    component: () => import('@/views/pages/NotificationsView.vue')
                },
                {
                    path: 'distributor-groups',
                    name: 'distributor-groups',
                    component: () => import('@/views/admin/UserGroup/index.vue')
                },
                {
                    path: 'unit-group',
                    name: 'unit-group',
                    component: () => import('@/views/admin/MasterData/UnitGroup/index.vue')
                },
                {
                    path: 'access-denied',
                    name: 'access-denied',
                    component: () => import('../views/pages/AccessDenied.vue')
                },
                // Cơ cấu tổ chức
                {
                    path: 'organizational-structure',
                    name: 'organizational-structure',
                    component: () => import('../views/admin/OrganizationalStructure/index.vue')
                },
                {
                    path: 'debt-reconciliation',
                    name: 'admin-debt-reconciliation',
                    component: () => import('../views/admin/DebtReconciliation/index.vue')
                },
                {
                    path: 'confirmation-minutes',
                    name: 'admin-confirmation-minutes',
                    component: () => import('../views/admin/ConfirmationMinutes/index.vue')
                },
                {
                    path: 'confirmation-minutes/:id',
                    name: 'admin-confirmation-minutes-detail',
                    component: () => import('../views/admin/ConfirmationMinutes/detail.vue')
                },
                {
                    path: 'payment-rule', // Thiết lập thanh toán
                    name: 'payment-rule',
                    component: () => import('../views/admin/PaymentRule/index.vue')
                },
                {
                    path: 'cms',
                    name: 'cms',
                    children: [
                        {
                            path: 'theme',
                            name: 'cms-theme',
                            component: () => import('../views/admin/CMS/Theme/index.vue')
                        },
                        {
                            path: 'menu',
                            name: 'cms-menu',
                            component: () => import('../views/admin/CMS/Menu/index.vue')
                        }
                    ]
                },
                {
                    path: 'notication',
                    name: 'notication',
                    component: () => import('../views/common/Notication/index.vue')
                }
            ]
        },
        //====================================================== [ CLIENT ROUTERS ] ======================================================
        {
            path: '/client',
            name: 'client',
            meta: { middleware: [auth] },
            component: Default,
            children: [
                {
                    path: 'setup',
                    name: 'client-profile',
                    component: () => import('@/views/client/pages/user_menu/index.vue'),
                    children: [
                        {
                            path: 'user',
                            name: 'user',
                            component: () => import('@/views/client/pages/user_menu/components/setupAccount.vue')
                        },
                        {
                            path: 'document',
                            name: 'user-document',
                            component: () => import('@/views/client/pages/user_menu/components/Document.vue')
                        },
                        {
                            path: 'hisPur',
                            name: 'hisPur',
                            component: () => import('@/views/client/pages/user_menu/components/historyPurchases.vue')
                        },
                        // {
                        //     path: 'return-request',
                        //     children: [
                        //         {
                        //             path: '',
                        //             name: 'clientReturnRequest',
                        //             component: () => import('@/views/client/pages/user_menu/components/ReturnRequest/ReturnRequestList.vue')
                        //         },
                        //         {
                        //             path: 'create-independent',
                        //             name: 'client-return-request-independent',
                        //             component: () => import('@/views/client/pages/user_menu/components/ReturnRequest/ReturnRequestIndependent.vue')
                        //         },
                        //         {
                        //             path: 'select-order',
                        //             name: 'client-return-request-select-order',
                        //             component: () => import('@/views/client/pages/user_menu/components/ReturnRequest/ReturnRequestSelectOrder.vue')
                        //         },
                        //         {
                        //             path: 'from-order/:id',
                        //             name: 'client-return-request-from-order',
                        //             component: () => import('@/views/client/pages/user_menu/components/ReturnRequest/ReturnRequestFromOrder.vue')
                        //         },
                        //         {
                        //             path: 'detail/:id',
                        //             name: 'client-return-request-detail',
                        //             component: () => import('@/views/common/ReturnRequest/ReturnRequestDetail.vue'),
                        //             props: { isClient: true }
                        //         }
                        //     ]
                        // },
                        {
                            path: 'hisPurNET',
                            children: [
                                {
                                    path: '',
                                    name: 'hisPurNET',
                                    component: () => import('@/views/client/pages/user_menu/components/TableOrderNET.vue')
                                },
                                // {
                                //     path: 'new',
                                //     name: 'hisPurNET-add',
                                //     component: () => import('@/views/client/pages/user_menu/components/OrderNETDetail.vue')
                                // },
                                // {
                                //     path: 'detail/:id',
                                //     name: 'hisPurNET-detail',
                                //     component: () => import('@/views/client/pages/user_menu/components/OrderNetEdit.vue')
                                // }
                            ]
                        },
                        {
                            path: 'production-commitment',
                            children: [
                                {
                                    path: '',
                                    name: 'production-commitment',
                                    component: () => import('@/views/client/pages/user_menu/components/productionCommitment.vue')
                                },
                                {
                                    path: ':id',
                                    name: 'production-commitment-detail',
                                    component: () => import('@/views/client/pages/PurchaseOrder/views/OrderDetailV1VPKM.vue')
                                }
                            ]
                        },
                        {
                            path: 'boardSetup',
                            name: 'boardSetup',
                            component: () => import('@/views/client/pages/user_menu/components/boardSetup.vue')
                        },
                        {
                            path: 'feedback',
                            name: 'feedback',
                            component: () => import('@/views/client/pages/user_menu/components/Feedback/index.vue')
                        },
                        {
                            path: 'inventory',
                            name: 'inventory',
                            component: () => import('@/views/client/pages/user_menu/components/Inventory.vue')
                        },
                        {
                            path: 'inventory-charge',
                            name: 'inventory-charge',
                            component: () => import('@/views/client/pages/user_menu/components/InventoryCharge.vue')
                        },
                        {
                            path: 'confirmation-minutes',
                            name: 'client-confirmation-minutes',
                            component: () => import('@/views/client/pages/report/DSBBXNSLHG/index.vue')
                        },
                        {
                            path: 'confirmation-minutes/:id',
                            name: 'client-confirmation-minutes-detail',
                            component: () => import('@/views/client/pages/report/DSBBXNSLHG/detail.vue')
                        },
                        {
                            path: 'notifications',
                            name: 'client-notifications',
                            component: () => import('@/views/pages/NotificationsView.vue')
                        },
                        {
                            path: 'purchase-request-list',
                            name: 'purchase-request-list',
                            component: () => import('@/views/client/pages/user_menu/components/PurchaseRequestList.vue')
                        },
                        {
                            path: 'purchase-request-client/:id',
                            name: 'detail-purchase-request',
                            component: () => import('@/views/client/pages/user_menu/components/DetailPurchaseRq.vue')
                        },
                        {
                            path: 'promotiondetail',
                            children: [
                                {
                                    path: '',
                                    name: 'promotiondetail-list',
                                    component: () => import('@/views/client/pages/user_menu/components/PromotiondetailListVPKM.vue')
                                },
                                {
                                    path: 'add',
                                    name: 'promotiondetail-add',
                                    component: () => import('@/views/client/pages/user_menu/components/Promotiondetail.vue')
                                }
                            ]
                        },

                        {
                            path: 'sent-warehouse',
                            name: 'sent-warehouse',
                            component: () => import('@/views/client/pages/user_menu/components/sentWarehouse.vue')
                        },
                        {
                            path: 'purchase-plan',
                            name: 'purchase-plan',
                            component: () => import('@/views/client/pages/user_menu/PurchasePlan/index.vue')
                        },
                        {
                            path: 'purchase-plan/:id',
                            name: 'purchase-plan-detail-client',
                            component: () => import('@/views/admin/OrderPlanning/DetailOrderPlan.vue')
                        },
                        // client/setup/...
                        {
                            path: 'debt-reconciliation',
                            name: 'debt-reconciliation-client',
                            component: () => import('../views/client/pages/DebtReconciliation/index.vue')
                        }
                    ],
                    meta: {
                        middleware: [auth]
                    }
                },

                {
                    path: 'signalr',
                    name: 'signalr',
                    component: () => import('@/views/pages/SignalR.vue')
                },
                {
                    path: 'detail/:id',
                    name: 'detail',
                    component: () => import('@/views/client/pages/detail/index.vue')
                },
                {
                    path: '/client',
                    name: 'client-home',
                    component: () => import('@/views/client/pages/home/index.vue')
                },

                {
                    path: 'categories',
                    name: 'categories',
                    component: () => import('@/views/client/pages/categories/index.vue'),
                    meta: {
                        middleware: [auth]
                    }
                },
                {
                    path: 'cart',
                    name: 'cart',
                    component: () => import('@/views/client/pages/cart/index.vue')
                },
                {
                    path: 'payment/:type/:id',
                    name: 'paymentMethod',
                    component: () => import('@/views/client/pages/cart/payment.vue')
                },
                {
                    path: 'success',
                    name: 'order-success',
                    component: () => import('@/views/client/pages/user_menu/components/SuccessPage.vue'),
                    meta: {
                        middleware: [auth]
                    }
                },
                {
                    path: 'order',
                    name: 'order',
                    component: () => import('@/views/client/pages/PurchaseOrder/index.vue'),
                    meta: {
                        middleware: [auth]
                    },
                    children: [
                        {
                            path: 'new-order',
                            name: 'client-create-new-order',
                            component: () => import('@/views/client/pages/PurchaseOrder/views/NewOrder.vue')
                        },
                        {
                            path: 'new',
                            name: 'client-create-order',
                            component: () => import('@/views/client/pages/PurchaseOrder/views/Home.vue')
                        },
                        {
                            path: ':id',
                            name: 'client-order-detail',
                            // component: () => import('@/views/client/pages/PurchaseOrder/views/OrderDetail.vue')
                            component: () => import('@/views/client/pages/PurchaseOrder/views/OrderDetailV1.vue')
                        },
                        {
                            path: 'check-out',
                            name: 'order-check-out',
                            component: () => import('@/views/client/pages/PurchaseOrder/views/CheckOut.vue')
                        },
                        {
                            path: 'purchase-order-copy/:id',
                            name: 'purchase-order-copy',
                            component: () => import('../views/common/PurchaseOrder/indexCopyOrder.vue')
                        },
                    ]
                },
                {
                    path: 'hisPurNET',
                    children: [
                        {
                            path: 'detail/:id',
                            name: 'hisPurNET-detail',
                            component: () => import('@/views/client/pages/user_menu/components/OrderNetEdit.vue')
                        },
                        {
                            path: 'new',
                            name: 'hisPurNET-add',
                            component: () => import('@/views/client/pages/user_menu/components/OrderNETDetail.vue')
                        },
                    ]
                },
                {
                    path: 'forcast',
                    name: 'client-create-forcast',
                    component: () => import('@/views/client/pages/forcast/createForcast.vue')
                },
                {
                    path: 'purchase-request',
                    name: 'client-create-purchasereq',
                    component: () => import('@/views/client/pages/purchaserequest/PurchaseRequest.vue')
                },

                //Report client
                {
                    path: 'report',
                    name: 'reportClient',
                    component: () => import('@/views/client/pages/report/index.vue')
                },
                {
                    path: 'report/buy-by-product', // Báo cáo - Mua hàng theo sản phẩm
                    name: 'client-buy-by-product',
                    component: () => import('@/views/client/pages/report/buyByProductCL.vue')
                },
                {
                    path: 'report/purchase-by-order', // Báo cáo - Mua hàng theo đơn hàng
                    name: 'client-purchase-by-order',
                    component: () => import('@/views/client/pages/report/PurchaseByOrder/index.vue')
                },
                {
                    path: 'report/debt-payment', // Báo cáo - Công nợ phải trả
                    name: 'client-debt-payment',
                    component: () => import('@/views/client/pages/report/DebtPayment/index.vue')
                },
                {
                    path: 'report/bill-summary', // Báo cáo - Thống kê hóa đơn
                    name: 'client-bill-summary',
                    component: () => import('@/views/client/pages/report/BillSummary/index.vue')
                },
                {
                    path: 'report/purchase-plan', // Báo cáo - Kế hoạch nhập hàng
                    name: 'client-purchase-plan',
                    component: () => import('@/views/client/pages/report/PurchasePlan/index.vue')
                },
                {
                    path: 'report/debt-reconciliation-data', // Báo cáo - Danh sách biên bản đối chiếu công nợ
                    name: 'client-debt-reconciliation-data',
                    component: () => import('@/views/client/pages/report/DebtReconciliationData/index.vue')
                },
                {
                    path: 'report/quantity-consigned-goods', // Báo cáo - DANH SÁCH BIÊN BẢN XÁC NHẬN SỐ LƯỢNG HÀNG GỬI
                    name: 'client-quantity-consigned-goods',
                    component: () => import('@/views/client/pages/report/DSBBXNSLHG/index.vue')
                },
                {
                    path: 'report/inventory-send',
                    name: 'inventorySendClient',
                    component: () => import('@/views/client/pages/report/goodInventorySendCL.vue')
                },
                {
                    path: 'report/inventory',
                    name: 'inventoryReportClient',
                    component: () => import('@/views/client/pages/report/inventoryReportCL.vue')
                },
                {
                    path: 'report/average-price',
                    name: 'averagePriceClient',
                    component: () => import('@/views/client/pages/report/averagePriceReportCL.vue')
                },
                {
                    path: 'report/commited',
                    name: 'commitedReportClient',
                    component: () => import('@/views/client/pages/report/committedOutputReportCL.vue')
                },
                {
                    path: 'report/pointPromotion',
                    name: 'pointPromotionReportClient',
                    component: () => import('@/views/client/pages/report/pointPromotionReportCL.vue')
                },

                // End report client
                // Payment status
                {
                    path: '/payment-status',
                    name: 'payment-status',
                    component: () => import('../views/pages/PaymentStatus.vue')
                },
                {
                    path: 'post',
                    name: 'post',
                    children: []
                }
            ]
        },
        // Guess Routers
        {
            path: '/preview-page',
            name: 'preview-page',
            component: GuessLayout,
            children: [
                {
                    path: '',
                    name: 'home-nologin',
                    component: () => import('@/views/client/pages/home_nologin/index.vue')
                },
                {
                    path: 'detail/:id',
                    name: 'detail-nologin',
                    component: () => import('@/views/client/pages/detail/no_login.vue')
                },
                {
                    path: 'gioi-thieu-doanh-nghiep',
                    name: 'gioi-thieu-doanh-nghiep',
                    component: () => import('../views/client/pages/posts/gioi-thieu-doanh-nghiep.vue')
                },
                {
                    path: 'chinh-sach-giao-dich-chung',
                    name: 'chinh-sach-giao-dich-chung',
                    component: () => import('../views/client/pages/posts/chinh-sach-giao-dich-chung.vue')
                },
                {
                    path: 'chinh-sach-doi-tra-bao-hanh',
                    name: 'chinh-sach-doi-tra-bao-hanh',
                    component: () => import('../views/client/pages/posts/chinh-sach-doi-tra-bao-hanh.vue')
                },
                {
                    path: 'chinh-sach-khieu-nai-ho-tro',
                    name: 'chinh-sach-khieu-nai-ho-tro',
                    component: () => import('../views/client/pages/posts/chinh-sach-khieu-nai-ho-tro.vue')
                },
                {
                    path: 'chinh-sach-bao-ve-thong-tin-ca-nhan',
                    name: 'chinh-sach-bao-ve-thong-tin-ca-nhan',
                    component: () => import('../views/client/pages/posts/chinh-sach-bao-ve-thong-tin-ca-nhan.vue')
                },
                {
                    path: 'chinh-sach-van-chuyen-giao-nhan',
                    name: 'chinh-sach-van-chuyen-giao-nhan',
                    component: () => import('../views/client/pages/posts/chinh-sach-van-chuyen-giao-nhan.vue')
                },
                {
                    path: 'chinh-sach-thanh-toan',
                    name: 'chinh-sach-thanh-toan',
                    component: () => import('../views/client/pages/posts/chinh-sach-thanh-toan.vue')
                },
                {
                    path: 'thong-tin-hang-hoa-dich-vu',
                    name: 'thong-tin-hang-hoa-dich-vu',
                    component: () => import('../views/client/pages/posts/thong-tin-hang-hoa-dich-vu.vue')
                },
                {
                    path: 'huong-dan-dat-hang',
                    name: 'huong-dan-dat-hang',
                    component: () => import('../views/client/pages/posts/huong-dan-dat-hang.vue')
                },
                {
                    path: 'dieu-khoan-su-dung',
                    name: 'dieu-khoan-su-dung',
                    component: () => import('../views/client/pages/posts/dieu-khoan-su-dung.vue')
                }
            ]
        },

        {
            path: '/login',
            name: 'login',
            component: () => import('@/views/auth/Login.vue')
        },
        {
            path: '/otp-verify',
            name: 'otp',
            component: () => import('@/views/auth/OTP.vue')
        },
        {
            path: '/forgot-password',
            name: 'forgot-password',
            component: () => import('@/views/auth/ForgotPassword.vue')
        },
        {
            path: '/reset-password',
            name: 'reset-password',
            component: () => import('@/views/auth/ResetPassword.vue')
        },

        {
            path: '/:pathMatch(.*)*',
            name: 'NotFound',
            component: () => import('@/views/pages/NotFound.vue')
        }
    ]
});

function nextFactory(context, middleware, index) {
    const subsequentMiddleware = middleware[index];
    if (!subsequentMiddleware) return context.next;
    return (...parameters) => {
        context.next(...parameters);

        const nextMiddleware = nextFactory(context, middleware, index + 1);
        subsequentMiddleware({ ...context, next: nextMiddleware });
    };
}
router.beforeEach((to, from, next) => {
    window.scrollTo(0, 0);
    to.redirectedFrom = from;
    if (to.meta.middleware) {
        const middleware = Array.isArray(to.meta.middleware) ? to.meta.middleware : [to.meta.middleware];
        const context = {
            from,
            next,
            router,
            to
        };
        const nextMiddleware = nextFactory(context, middleware, 1);
        return middleware[0]({ ...context, next: nextMiddleware });
    }
    return next();
});

export default router;
