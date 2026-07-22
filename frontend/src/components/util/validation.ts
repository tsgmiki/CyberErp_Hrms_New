import { z } from "zod";

const MAX_UPLOAD_SIZE = 1024 * 1024 * 2; // 3MB
const phoneRegex = new RegExp(
  /^([+]?[\s0-9]+)?(\d{3}|[(]?[0-9]+[)])?([-]?[\s]?[0-9])+$/,
);

export const DOCUMENT_SCHEMA = z
  .instanceof(File)
  .refine(
    (file) =>
      [
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/svg+xml",
        "image/gif",
        "application/pdf",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      ].includes(file.type),
    { message: "Invalid document file type" },
  )
  .refine((file) => file.size <= MAX_UPLOAD_SIZE, {
    message: "File size should not exceed 2MB",
  });

// Image Schema
export const IMAGE_SCHEMA = z
  .instanceof(File)
  .refine(
    (file) =>
      [
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/svg+xml",
        "image/gif",
      ].includes(file.type),
    { message: "Invalid image file type" },
  );
export const LoginSchema = z.object({
  userName: z.string().min(2, "userName is Required").max(255),
  password: z.string().min(1).max(255),
});

export const DocumentTemplateSchema = z.object({
  name: z.string().min(1, "Template name is required").max(200),
  documentType: z.string().min(1, "Document type is required"),
  // Optional string fields must also accept null: on edit the backend returns null
  // (not "") for unset nullable columns, and z.string().optional() rejects null.
  headerHtml: z.string().max(100000).nullish(),
  body: z
    .string("Template body is required")
    .min(1, "Template body is required"),
  footerHtml: z.string().max(100000).nullish(),
  description: z.string().max(1000).nullish(),
  isActive: z.union([z.boolean(), z.string()]).nullish(),
  id: z.string().nullish(),
});
export const PasswordSchema = z.object({
  newPassword: z.string().min(1, "Password is required").max(255),
  oldPassword: z.string().min(1).max(255),
  confirmPassword: z.string().min(1).max(255),
});
export const SignupSchema = z.object({
  fullName: z.string().min(2, "Full Name is Required").max(200),
  email: z
    .string()
    .email()
    .min(1, "Email is Required")
    .max(200, "Email must be maximum of 500 characters"),
  phoneNo: z
    .string()
    .min(1, "phoneNo is Required")
    .max(200, "phoneNo must be maximum of 500 characters")
    .regex(phoneRegex, "Invalid Phone Number!"),

  companyName: z.string().max(500, "company must be maximum of 500 characters"),
  type: z.string().max(500, "company type must be maximum of 500 characters"),
  address: z.string().max(500, "address  must be maximum of 500 characters"),

  password: z
    .string()
    .min(8, "Password must be minimum of 8 characters")
    .max(255)
    .regex(/[a-z]/, "Password must contain a lowercase letter")
    .regex(/[A-Z]/, "Password must contain an uppercase letter")
    .regex(/\d/, "Password must contain a number")
    .regex(/[!@#$%^&*()]/, "Password must contain a special character"),
});
export const UserSchema = z
  .object({
    fullName: z.string().min(2, "Full Name is Required").max(200),
    userName: z.string().min(1, "User Name is Required"),
    email: z
      .string()
      .email()
      .min(1, "Email is Required")
      .max(200, "Email must be maximum of 500 characters"),
    phoneNumber: z
      .string()
      .min(1, "phoneNo is Required")
      .max(200, "phoneNo must be maximum of 500 characters"),

    id: z.string().optional(),
    password: z.string().optional(),
  })
  .refine(
    (data) => {
      // If creating (no ID), password must be present and valid
      if (!data.id) {
        const pwd = data.password ?? "";
        return (
          pwd.length >= 8 &&
          pwd.length <= 255 &&
          /[a-z]/.test(pwd) &&
          /[A-Z]/.test(pwd) &&
          /\d/.test(pwd) &&
          /[!@#$%^&*()]/.test(pwd)
        );
      }
      // If editing (has ID), password can be empty
      return true;
    },
    {
      message:
        "Password must be at least 8 characters and include uppercase, lowercase, number, and special character",
      path: ["password"],
    },
  );

export const ModuleSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  subsystemId: z.string().min(2, "Sub System is Required").max(200),
});

export const SubsystemSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  code: z.string().min(1, "Code is Required").max(50),
});

export const OperationSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  moduleId: z.string().min(2, "Module is Required").max(200),
  link: z.string().min(2, "Link is Required").max(200),
});
export const RoleSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
});
export const UserRoleSchema = z.object({
  userId: z.string().min(2, "User is Required").max(200),
  roleId: z.string().min(2, "User is Required").max(200),
});
export const RolePermissionSchema = z.object({
  roleId: z.string().min(2, "Role is Required").max(200),
});

export const ItemCategorySchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
});
export const ItemSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  code: z.string().max(200),
  itemType: z.string().min(1).max(200),

  specification: z.string().max(2000),
  itemCategoryId: z.string().min(2, "Item Category is Required").max(200),
});
export const TaxTypeSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  rate: z.number(),
});
export const StoreSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
});
export const StoreUserSchema = z.object({
  storeId: z.string().min(2, "Store is Required").max(200),
  userId: z.string().min(2, "User is Required").max(200),
});
export const BincardSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  storeId: z.string().min(2, "Store is Required").max(200),
  date: z.string().min(2, "Date is Required").max(200),
  voucherType: z.string().min(2, "Voucher Type is Required").max(200),
  issuedQuantity: z.number(),
  receivedQuantity: z.number(),
  unitCost: z.number().nullable(),
});
export const OpeningBalanceSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  storeId: z.string().min(2, "Store is Required").max(200),
  quantity: z.number().gt(0),
  unitCost: z.number(),
});
export const DefaultSettingSchema = z.object({
  storeId: z.string().min(2, "Store is Required").max(200),
});
export const CompanySchema = z.object({
  name: z.string().min(2, "Company Name is Required").max(200),
  address: z.string().min(2, "Address is Required").max(500),
});
export const SalesOrderSchema = z
  .object({
    customerId: z.string().min(2, "Customer is Required").max(200),
    storeId: z.string().min(2, "Store is Required").max(200),
    salesType: z.string().min(2, "Sales Type is Required").max(200),
    date: z.string().min(2, "Date is Required").max(200),
    bankId: z.string(),
    amount: z.number(),
    banks: z.string(),
  })
  .refine(
    (data) => !(data.salesType == "Cash" && !(data.bankId || data.banks)),
    {
      message: "Bank is Required",
      path: ["banks", "bankId"], // path of error
    },
  );

export const DailySummarySchema = z.object({
  salesAmount: z.number().gte(0),
  expense: z.number().gte(0),
  bankDeposite: z.number().gte(0),
  salesReturnAMount: z.number().gte(0),
  unDepositedAmount: z.number().gte(0),
  difference: z.number().gte(0),
});

export const PurchaseOrderSchema = z
  .object({
    supplierId: z.string().min(2, "Supplier is Required").max(200),
    coffeeSupplierId: z.string().max(200).optional(),
    storeId: z.string().min(2, "Store is Required").max(200),
    itemId: z.string().min(1, "Item is Required").max(200),
    quantity: z.number().gt(0),
    unitPrice: z.number().gte(0),
    date: z.string().min(2, "Date is Required").max(200),
    banks: z.string(),
  })
  .refine((data) => !!data.banks, {
    message: "Bank is Required",
    path: ["banks", "bankId"], // path of error
  });

export const ReceiveSchema = z.object({
  supplierId: z.string().min(2, "Supplier is Required").max(200),
  storeId: z.string().min(2, "Store is Required").max(200),
  itemId: z.string().min(1, "Item is Required").max(200),
  quantity: z.number().gt(0),
  unitPrice: z.number().gte(0),
  date: z.string().min(2, "Date is Required").max(200),
  amount: z.number(),
});

export const SupplierSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  tin: z.string().max(200),
  phoneNo: z.string().max(200),
  email: z.string().max(200),
  address: z.string().max(500),
  purchaserId: z.string().max(200).optional(),
});

export const AccountGroupSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  accountType: z.string().min(2, "Account Type is Required").max(200),
});

export const AccountSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  accountGroup: z.string().min(2, "account Group is Required").max(200),
  accountType: z.string().min(2, "Account Type is Required").max(200),
  code: z.string().min(2, "Code is Required").max(200),
  balanceSide: z.string().min(2, "Balance Side is Required").max(200),
  balance: z.number(),
});

export const AccountTransactionSchema = z.object({
  accountId: z.string().min(2, "Account is Required").max(200),
  referenceNo: z.string().min(2, "Reference No is Required").max(200),
  date: z.string().min(2, "Date is Required").max(200),
  code: z.string().min(2, "Code is Required").max(200),
  balanceSide: z.string().min(2, "Balance Side is Required").max(200),
  debit: z.number(),
  credit: z.number(),
});

export const DocumentSettingSchema = z.object({
  voucherType: z.string().min(2, "Voucher Type is Required").max(200),
  lastNumber: z.number(),
  prefix: z.string().max(200),
  sufix: z.string().max(200),
});

export const CollectionSchema = z
  .object({
    customerId: z.string().min(2, "Customer is Required").max(200),
    bankId: z.string(),

    reason: z.string().min(2, "reason is Required").max(200),
    date: z.string().min(2, "Date is Required").max(200),
    amount: z.number(),
    banks: z.string(),
  })
  .refine((data) => !!(data.bankId || data.banks), {
    message: "Banks is Required",
    path: ["banks", "bankId"], // path of error
  });

export const CollectionBankSchema = z.object({
  bankId: z.string().min(2, "Bank is Required").max(200),
  amount: z.number().gt(0),
});
export const CollectionDetailSchema = z.object({
  salesOrderId: z.string().min(2, "Sales Order is Required").max(200),
  amount: z.number(),
  amountRemaining: z.number(),
});
export const SalesPersonSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  phoneNo: z.string().max(200),
});
export const BankSchema = z
  .object({
    name: z.string().min(2, "Name is Required").max(200),
    code: z.string().min(2, "Code is Required").max(200),
    supplierId: z.string(),
    pettyCashId: z.string(),
    accountType: z.string().min(2, "Account Type is Required").max(200),
  })
  .refine(
    (data) => !(data.accountType == "purchase fund" && !data.supplierId),
    {
      message: "supplier is Required",
      path: ["supplierId"], // path of error
    },
  )
  .refine(
    (data) => !(data.accountType == "pettycash fund" && !data.pettyCashId),
    {
      message: "pettyCash is Required",
      path: ["pettyCashId"], // path of error
    },
  );

export const SalesReportSchema = z.object({
  //fromDate: z.string().min(2, "From Date is Required").max(200),
  //toDate: z.string().min(2, "To Date is Required").max(200),
});
export const LedgerReportSchema = z.object({
  //fromDate: z.string().min(2, "From Date is Required").max(200),
  //toDate: z.string().min(2, "To Date is Required").max(200),
});

export const PurchaseReportSchema = z.object({
  // fromDate: z.string().min(2, "From Date is Required").max(200),
  //toDate: z.string().min(2, "To Date is Required").max(200),
});
export const ExpenseReportSchema = z.object({
  // fromDate: z.string().min(2, "From Date is Required").max(200),
  //toDate: z.string().min(2, "To Date is Required").max(200),
});
export const UserPermissionSchema = z.object({
  userId: z.string().min(2, "user is Required").max(200),
});
export const ExpensePurposeSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  expenseType: z.string().min(2, "Expense Type is Required").max(200),
});
export const ExpenseTypeSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
});

export const InventoryReportSchema = z.object({});

export const SummaryReportSchema = z.object({});
export const LookupCategorySchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
});
export const LookupSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  code: z.string().max(200).optional(),
  remark: z.string().max(2000).optional(),
  tableName: z.string().max(200).optional(),
});
export const ServiceRateSchema = z.object({
  serviceTypeId: z.string().min(1, "Service Type is Required").max(200),
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
  currencyId: z.string().min(1, "Currency is Required").max(200),
  unitId: z.string().min(1, "Unit is Required").max(200),

  rate: z.number().min(0, "Rate must be positive"),
  description: z.string().max(2000).optional(),
});
export const FiscalYearSchema = z.object({
  name: z.string().min(2, "Name is Required").max(100),
  startDate: z.string().min(1, "Start Date is Required"),
  endDate: z.string().min(1, "End Date is Required"),
});

export const UnitSchema = z.object({
  name: z.string().min(1).max(200),
  location: z.string().min(1).max(200),
  parentUnitId: z.string().max(200),
});
export const WorkflowSchema = z.object({
  voucherType: z.string().min(1).max(200),
  step: z.number(),
  statusId: z.string().min(1).max(200),
  criteria: z.string().max(2000),
});
export const JobOrderTemplateSchema = z.object({
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
  name: z.string().min(1, "Name is Required").max(200),
  description: z.string().max(2000),
  value: z.string().max(2000),
});
export const JobOrderSchema = z.object({
  date: z.string().min(1, "Date is Required").max(200),
  customerId: z.string().min(1, "Customer is Required").max(200),
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
  ultimateClient: z.string().max(200),
  receivingClient: z.string().max(200),
  contactPersonLocal: z.string().max(200),
  contactPersoForeign: z.string().max(200),
  referenceNo: z.string().max(200),
  validityDate: z.string().min(1, "ValidityDate is Required").max(200),

  remark: z.string().max(2000),
});
export const JobOrderDetailSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  description: z.string().max(2000),
  value: z.string().max(2000),
});
export const ApproverSchema = z.object({
  voucherType: z.string().min(1).max(200),
  statusId: z.string().min(1, "Status is Required").max(200),
  approverId: z.string().min(1, "Approver is Required").max(200),
  criteria: z.string().max(2000),
});
export const StoreApproverSchema = z.object({
  voucherType: z.string().min(1).max(200),
  status: z.string().min(1).max(200),
  userId: z.string().min(1).max(200),
});
export const DeligationSchema = z.object({
  userId: z.string().min(1).max(200),
  deligatedUserId: z.string().min(1).max(200),
  startDate: z.string().min(1).max(200),
  endDate: z.string().min(1).max(200),
});

// (legacy inventory EmployeeSchema removed — superseded by the HRMS EmployeeSchema below)

export const StoreRequisitionSchema = z
  .object({
    unitId: z.string().max(200),
    toStoreId: z.string().max(200),
    employeeId: z.string().max(200),
    storeId: z.string().min(1, "Store is Required").max(200),
    requestType: z.string().min(1).max(200),
    date: z.string().min(2, "Date is Required").max(200),
  })
  .refine((data) => !(data.requestType == "Unit" && !data.unitId), {
    message: "Unit is Required",
    path: ["unitId"], // path of error
  })
  .refine((data) => !(data.requestType == "store" && !data.toStoreId), {
    message: "to store is Required",
    path: ["toStoreId"], // path of error
  })
  .refine((data) => !(data.requestType == "employee" && !data.employeeId), {
    message: "employee is Required",
    path: ["employeeId"], // path of error
  });
export const StoreRequisitionDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  remark: z.string().max(2000),
  quantity: z.number().gt(0),
});
export const IssueSchema = z.object({
  requestedById: z.string().min(1, "Requested By is Required").max(200),
  plateNo: z.string().max(200),
  driver: z.string().max(200),
  storeId: z.string().min(1, "Store is Required").max(200),
  itemId: z.string().min(1, "Item is Required").max(200),
  quantity: z.number().gt(0),
  date: z.string().min(2, "Date is Required").max(200),
  remark: z.string().max(2000),
});
export const DeliverySchema = z.object({
  requestedById: z.string().min(1, "Requested By is Required").max(200),
  deliveredById: z.string().max(200),
  plateNo: z.string().max(200),
  driver: z.string().max(200),
  storeId: z.string().min(1, "Store is Required").max(200),
  itemId: z.string().min(1, "Item is Required").max(200),
  quantity: z.number().gt(0),
  date: z.string().min(2, "Date is Required").max(200),
  remark: z.string().max(2000),
});
export const IssueDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  remark: z.string().max(2000),
  quantity: z.number().gt(0),
});
export const IssueAssetDetailSchema = z.object({
  assetId: z.string().min(2, "Asset is Required").max(200),
});

export const StatusSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  code: z.string().min(1, "Code is Required").max(200),
  dataTypeId: z.string().min(1, "Data Type is Required").max(200),
  plannedDuration: z.number().min(0, "Planned Duration must be positive"),
  description: z.string().max(2000).optional(),
  remark: z.string().max(2000).optional(),
  openToClient: z.boolean().optional(),
  statusType: z.string().max(200).optional(),
});
export const StatusTemplateStatusSchema: z.ZodType<any> = z.object({
  statusId: z.string().min(1, "Status is Required").max(200),
});

export const StatusTemplateByLIstSchema: z.ZodType<any> = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
  roleId: z.string().min(1, "Role is Required").max(200),
  details: z
    .array(StatusTemplateStatusSchema)
    .min(1, "The items array cannot be empty"),
});
export const StatusTemplateSchema: z.ZodType<any> = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
  roleId: z.string().min(1, "Role is Required").max(200),
});

export const OperationStatuStatusSchema: z.ZodType<any> = z.object({
  statusId: z.string().min(1, "Status is Required").max(200),
  value: z.string().max(200),
});

export const OperationStatusByLIstSchema: z.ZodType<any> = z.object({
  details: z
    .array(OperationStatuStatusSchema)
    .min(1, "The items array cannot be empty"),
});
export const OperationStatusSchema: z.ZodType<any> = z.object({
  statusId: z.string().min(1, "Status is Required").max(200),
  value: z.string().max(200),
});

export const OperationConfigFieldSchema: z.ZodType<any> = z.object({
  fieldName: z.string().min(1, "Field Name is Required").max(200),
  fieldCaption: z.string().min(1, "Field Caption is Required").max(200),
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
});
export const OperationConfigByListSchema: z.ZodType<any> = z.object({
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
  details: z
    .array(OperationConfigFieldSchema)
    .min(1, "The items array cannot be empty"),
});
export const SettingOperationSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  link: z.string().min(1, "Link is Required").max(200),
  ports: z.string().optional(),
  country: z.string().optional(),
});

export const JobOrderOperationSchema = z.object({
  jobOrderId: z.string().min(1, "Job Order is Required").max(200),
  date: z.string().min(1, "Date is Required").max(200),
  clientReferenceNo: z.string().max(200).optional(),
  goodsDescription: z.string().max(200).optional(),
  mblNo: z.string().max(200).optional(),
  hblNo: z.string().max(200).optional(),
  ssLine: z.string().max(200).optional(),
  vessel: z.string().max(200).optional(),
  voyage: z.string().max(200).optional(),
  portOfOriginId: z.string().max(200).optional(),
  portOfDestinationId: z.string().max(200).optional(),
  openingLocationId: z.string().max(200).optional(),
  etaPort: z.string().max(200).optional(),
  portDischargeDate: z.string().max(200).optional(),
  numberOfContainers: z.string().max(200).optional(),
  numberOfPackages: z.string().max(200).optional(),
  weight: z.number().optional(),
  volume: z.number().optional(),
  loadingPortId: z.string().max(200).optional(),
  branchId: z.string().min(1, "Branch is Required").max(200),
  transitorId: z.string().max(200).optional(),
  financeOfficerId: z.string().max(200).optional(),
  marketingOfficerId: z.string().max(200).optional(),
  cargoAgentId: z.string().max(200).optional(),
  boxFile: z.string().max(200).optional(),
  fileNo: z.string().max(200).optional(),
  mawbNo: z.string().max(200).optional(),
  hawbNo: z.string().max(200).optional(),
  carrier: z.string().max(200).optional(),
  flightNo: z.string().max(200).optional(),
  loadingAir: z.string().max(200).optional(),
  etd: z.string().max(200).optional(),
  eta: z.string().max(200).optional(),
  remarks: z.string().max(2000).optional(),
  containerClosingDate: z.string().max(200).optional(),
  vesselArrivalDate: z.string().max(200).optional(),
  doSecuredDate: z.string().max(200).optional(),
  portOfDischarge: z.string().max(200).optional(),
  declarationNo: z.string().max(200).optional(),
});

export const TransferIssueSchema = z.object({
  storeRequisitionId: z.string().max(200),
  plateNo: z.string().max(200),
  driver: z.string().max(200),
  storeId: z.string().min(1, "Store is Required").max(200),
  toStoreId: z.string().min(1, "to Store is Required").max(200),
  date: z.string().min(2, "Date is Required").max(200),
  remark: z.string().max(2000),
});
export const TransferIssueDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  remark: z.string().max(2000),
  quantity: z.number().gt(0),
});
export const TransferIssueAssetDetailSchema = z.object({
  assetId: z.string().min(2, "Asset is Required").max(200),
  quantity: z.number().gt(0),
});

export const TransferReceiveSchema = z.object({
  transferIssueId: z.string().max(200),
  plateNo: z.string().max(200),
  driver: z.string().max(200),
  storeId: z.string().min(1, "Store is Required").max(200),
  fromStoreId: z.string().min(1, "from store is Required").max(200),
  date: z.string().min(2, "Date is Required").max(200),
  remark: z.string().max(2000),
});
export const TransferReceiveDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  remark: z.string().max(2000),
  quantity: z.number().gt(0),
});
export const TransferReceiveAssetDetailSchema = z.object({
  assetId: z.string().min(2, "Asset is Required").max(200),
  quantity: z.number().gt(0),
});

export const PurchaseRequestSchema = z
  .object({
    storeRequisitionId: z.string().max(200),
    purchaseType: z.string().max(200),
    purchaserId: z.string().max(200),
    id: z.string(),
    storeId: z.string().min(1, "Store is Required").max(200),
    date: z.string().min(2, "Date is Required").max(200),
    remark: z.string().max(2000),
  })
  .refine((data) => !(data.id && !data.purchaseType), {
    message: "Purchase Type is Required",
    path: ["purchaseType"], // path of error
  })
  .refine((data) => !(data.id && !data.purchaserId), {
    message: "purchaser is Required",
    path: ["purchaserId"], // path of error
  });
export const PurchaseRequestDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  remark: z.string().max(2000),
  quantity: z.number().gt(0),
});
export const IssueEmployeeSchema = z.object({
  employeeId: z.string().min(2, "Employee is Required").max(200),
  issueId: z.string().min(2, "Issue is Required").max(200),
  remark: z.string().max(2000),
});
export const CustomerSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  code: z.string().min(1, "Code is Required").max(200),

  address: z.string().min(1, "Address is Required").max(500),
  countryId: z.string().min(1, "Country is Required").max(200),
  webSite: z.string().max(200).optional(),
  email: z.string().min(1, "Email is Required").email().max(200),
  phoneNumber: z.string().min(1, "Phone No is Required").max(200).optional(),
  parentCustomerId: z.string().max(200).optional(),
  customerCategoryId: z.string().max(200).optional(),
});
export const ContactPersonSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  customerId: z.string().min(1, "Customer is Required").max(200),
  email: z.string().email().max(200).optional().or(z.literal("")),
  phoneNumber: z.string().max(200).optional(),
  location: z.string().max(200).optional(),
  address: z.string().max(500).optional(),
  position: z.string().max(200).optional(),
});
export const TermsAndConditionSchema = z.object({
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
  description: z.string().min(1, "Description is Required").max(20000),
});
export const CustomerBranchSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  customerId: z.string().min(2, "Customer is Required").max(200),
  storeId: z.string().min(2, "Store is Required").max(200),

  area: z.number().gt(0),
  noofToilet: z.number().gt(0),
  remark: z.string().max(2000),
});

export const AdjustmentSchema = z.object({
  adjustmentType: z.string().max(200),
  otherReferenceNo: z.string().max(200),
  reason: z.string().max(200),
  storeId: z.string().min(1, "Store is Required").max(200),
  itemId: z.string().min(1, "Item is Required").max(200),
  quantity: z.number().gt(0),
  date: z.string().min(2, "Date is Required").max(200),
  remark: z.string().max(2000),
});
export const AdjustmentDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  // remark: z.string().max(2000),
  quantity: z.number().gt(0),
});
export const ReturnSchema = z.object({
  returnType: z.string().max(200),
  otherReferenceNo: z.string().max(200),
  reason: z.string().max(200),
  unitId: z.string().max(200),
  employeeId: z.string().max(200),
  itemId: z.string().min(1, "Item is Required").max(200),
  quantity: z.number().gt(0),
  storeId: z.string().min(1, "Store is Required").max(200),
  date: z.string().min(2, "Date is Required").max(200),
  remark: z.string().max(2000),
});
export const ReturnDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  //remark: z.string().max(2000),
  quantity: z.number().gt(0),
});
export const ReturnAssetDetailSchema = z.object({
  assetId: z.string().min(2, "Asset is Required").max(200),
  quantity: z.number().gt(0),
});

export const StoreRequisitionEmployeeSchema = z.object({
  employeeId: z.string().min(2, "Employee is Required").max(200),
  storeRequisitionId: z
    .string()
    .min(2, "Store Requisition is Required")
    .max(200),
  remark: z.string().max(2000),
});
export const StoreRequisitionCustomerBranchSchema = z.object({
  customerBranchId: z.string().min(2, "Customer Branch is Required").max(200),
  storeRequisitionId: z
    .string()
    .min(2, "Store Requisition is Required")
    .max(200),
  remark: z.string().max(2000),
});
export const ProductionMappingSchema = z.object({
  itemId: z.string().min(1).max(200),
  fgItemId: z.string().min(1).max(200),
  ratio: z.number().gt(0),
  storeId: z.string().min(1).max(200),
});
export const ProductionReceiveSchema = z.object({
  storeId: z.string().min(2, "Store is Required").max(200),
  receiveType: z.string().min(2, "Sales Type is Required").max(200),
  //  issueId: z.string().max(200),
  itemId: z.string().min(1, "Item is Required").max(200),
  quantity: z.number().gt(0),
  driver: z.string().max(200),
  plateNo: z.string().max(200),
  date: z.string().min(2, "Date is Required").max(200),
});
export const ProductionReceiveDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  quantity: z.number().gt(0),
  unitPrice: z.number().gte(0),
});
export const ExpenseSchema = z
  .object({
    pettyCashId: z.string(),
    storeId: z.string().min(1, "Store is Required").max(200),
    expenseType: z.string().min(1, "Expense Type is Required").max(200),
    reason: z.string().max(200),
    paidTo: z.string().max(200),
    date: z.string().min(1, "Date is Required").max(200),
    amount: z.number(),
    banks: z.string(),
  })
  .refine((data) => !!data.banks, {
    message: "Bank is Required",
    path: ["banks", "bankId"], // path of error
  });

export const PaymentSchema = z
  .object({
    supplierId: z.string().max(200).optional(),
    pettyCashId: z.string().max(200).optional(),
    paymentRequestId: z.string().min(1, "Payment Request is Required").max(200),
    paymentType: z.string().min(1).max(200),
    reason: z.string().min(2, "Reason is Required").max(200),
    date: z.string().min(2, "Date is Required").max(200),
    amount: z.number(),
    banks: z.string(),
  })
  .refine(
    (data) => {
      return data.paymentType !== "purchase fund" || !!data.supplierId;
    },
    {
      message: "Supplier is Required",
      path: ["supplierId"],
    },
  )
  .refine(
    (data) => {
      return data.paymentType !== "pettycash fund" || !!data.pettyCashId;
    },
    {
      message: "PettyCash is Required",
      path: ["pettyCashId"],
    },
  )
  .refine((data) => !!data.banks, {
    message: "Bank is Required",
    path: ["banks", "bankId"], // path of error
  });

export const PettyCashSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  code: z.string().max(200),
  purchaserId: z.string().max(200).optional(),
  storeId: z.string().min(2, "Store is Required").max(200),
});
export const AssetSchema = z.object({
  code: z.string().min(2, "Asset Code is Required").max(200),
  itemId: z.string().min(2, "Item is Required").max(200),
  storeId: z.string().min(2, "Store is Required").max(200),
  description: z.string().max(2000),
  plateNumber: z.string().max(200),
  serialNumber: z.string().max(200),
  engineNumber: z.string().max(200),
  grnDate: z.string().max(200),
  grnNumber: z.string().max(200),
  accDepCostBegining: z.number(),
  purchaseCost: z.number(),
  otherCost: z.number(),
  quantity: z.number(),
  isOut: z.boolean(),
  isDisposed: z.boolean(),
  remark: z.string().max(2000),
});
export const AssetCustodianSchema = z
  .object({
    assetId: z.string().min(2, "Asset is Required").max(200),
    unitId: z.string().max(200),
    employeeId: z.string().max(200),
    voucherType: z.string().max(200),
    voucherId: z.string().max(200),
    voucherNo: z.string().max(200),
    isReturned: z.boolean(),
    remark: z.string().max(2000),
  })
  .refine(
    (data) => {
      return !(!data.employeeId && !data.employeeId);
    },
    {
      message: "Custodian is Required",
      path: ["unitId", "employeeId"],
    },
  );
export const DepreciationSettingSchema = z.object({
  itemCategoryId: z.string().min(2, "Item Category is Required").max(200),
  rate: z.number(),
  specialRate: z.number(),
  remark: z.string().max(2000),
});
export const DepreciationSchema = z.object({
  assetId: z.string().min(1, "Asset is Required").max(200),
  storeId: z.string().min(1, "Store is Required").max(200),
  rate: z.number(),
  amount: z.number(),
  fromDate: z.string().min(1, "From Date is Required").max(200),
  toDate: z.string().min(1, "To Date is Required").max(200),
  remark: z.string().max(2000),
});

export const DisposeSchema = z.object({
  disposeType: z.string().max(200),
  otherReferenceNo: z.string().max(200),
  reason: z.string().max(200),
  storeId: z.string().min(1, "Store is Required").max(200),
  date: z.string().min(2, "Date is Required").max(200),
  remark: z.string().max(2000),
});
export const DisposeDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  remark: z.string().max(2000),
  quantity: z.number().gt(0),
});
export const DisposeAssetDetailSchema = z.object({
  assetId: z.string().min(2, "Asset is Required").max(200),
  quantity: z.number().gt(0),
});

export const AssetTransferSchema = z.object({
  assetTransferType: z.string().max(200),
  otherReferenceNo: z.string().max(200),
  reason: z.string().max(200),
  employeeId: z.string().min(1, "Employee is Required").max(200),
  toEmployeeId: z.string().min(1, "To Employee is Required").max(200),
  storeId: z.string().min(1, "Store is Required").max(200),
  date: z.string().min(2, "Date is Required").max(200),
  remark: z.string().max(2000),
});
export const AssetTransferDetailSchema = z.object({
  itemId: z.string().min(2, "Item is Required").max(200),
  remark: z.string().max(2000),
  quantity: z.number().gt(0),
});
export const AssetTransferAssetDetailSchema = z.object({
  assetId: z.string().min(2, "Asset is Required").max(200),
  quantity: z.number().gt(0),
});

export const CoffeeSupplierSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  tin: z.string().max(200),
  phoneNo: z.string().max(200),
  email: z.string().max(200),
  address: z.string().max(500),
});

export const DocumentCategorySchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
});
export const StorePermissionSchema = z.object({
  userId: z.string().min(2, "Name is Required").max(200),
});

export const DocumentSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  code: z.string().max(200),
  documentType: z.string().min(1).max(200),
  description: z.string().max(2000),
  documentCategoryId: z
    .string()
    .min(2, "Document Category is Required")
    .max(200),
});
export const PaymentRequestSchema = z
  .object({
    paymentType: z.string().max(200),
    id: z.string(),
    supplierId: z.string().max(200).optional(),
    pettyCashId: z.string().max(200).optional(),
    communicatedThrough: z.string().max(200).optional(),
    storeId: z.string().min(1, "Store is Required").max(200),
    date: z.string().min(2, "Date is Required").max(200),
    remark: z.string().max(2000),
    amount: z.number().gt(0),
  })
  .refine(
    (data) => {
      return data.paymentType !== "purchase fund" || !!data.supplierId;
    },
    {
      message: "Supplier is Required",
      path: ["supplierId"],
    },
  )
  .refine(
    (data) => {
      return data.paymentType !== "pettycash fund" || !!data.pettyCashId;
    },
    {
      message: "PettyCash is Required",
      path: ["pettyCashId"],
    },
  );
export const BankSelectorSchema = z.object({
  bankId: z.string().min(2, "Bank is Required").max(200),
  amount: z.number().gt(0),
});
export const ExpensePurposeSelectorSchema = z.object({
  expensePurposeId: z.string().min(2, "Expense Purpose is Required").max(200),
  amount: z.number().gt(0),
});
export const ExpenseDetailSchema = z.object({
  amount: z.number().gt(0),
  remark: z.string().max(4000).optional(),
  expensePurposeId: z.string().min(2, "Expense Purpose is Required").max(200),
});

export const QuotationSchema = z.object({
  date: z.string().min(1, "Date is Required").max(200),
  customerId: z.string().min(1, "Customer is Required").max(200),
  contactPersonId: z.string().min(1, "Contact Person is Required").max(200),
  referenceNo: z.string().max(200).optional(),
  validityDate: z
    .string()
    .min(1, "Validity Date is Required")
    .max(200)
    .optional(),
  remark: z.string().max(2000).optional(),
});

export const QuotationDetailSchema = z.object({
  quotationHeaderId: z.string().max(200).optional(),
  serviceRateId: z.string().min(1, "Service Rate is Required").max(200),
  rate: z.number().optional(),
  quantity: z.number().optional(),
  description: z.string().max(2000).optional(),
  sign: z.string().max(200).optional(),
  currencyId: z.string().max(200).optional(),
  operationTypeId: z.string().max(200).optional(),
  unitId: z.string().max(200).optional(),
});
export const QuotationTermsAndConditionSchema = z.object({
  description: z
    .string()
    .min(1, "Description is Required")
    .max(20000)
    .optional(),
});

export const ServiceRequestSchema = z.object({
  requester: z.string().min(1, "Requester is Required").max(200).optional(),
  date: z.string().min(1, "Date is Required").max(200),
  requestDate: z
    .string()
    .min(1, "Request Date is Required")
    .max(200)
    .optional(),
  customerId: z.string().max(200).optional(),
  contactPersonId: z.string().max(200).optional(),
  referenceNo: z.string().max(200).optional(),
  voucherNumber: z.string().max(200).optional(),
  address: z.string().max(200).optional(),
  commodityDescription: z.string().max(2000).optional(),
  responseDate: z.string().max(200).optional(),
  remark: z.string().max(2000).optional(),
});

export const ServiceRequestDetailSchema = z.object({
  serviceRequestHeaderId: z.string().max(200).optional(),
  commodityTypeId: z.string().min(1, "Commodity Type is required").max(200),
  containerTypeId: z.string().min(1, "Container Type is required").max(200),
  quantity: z.number().gt(0),
  description: z.string().max(2000).optional(),
  volume: z.number().optional(),
  weight: z.number().optional(),
  pcs: z.number().optional(),
  operationTypeId: z.string().min(1, "Container Type is required").max(200),
  pOE: z.string().max(200).optional(),
  pOD: z.string().max(200).optional(),
  tareWeight: z.string().max(200).optional(),
  cbm: z.number().optional(),
});

export const ContainerSchema = z.object({
  containerNo: z.string().min(1, "Container No is Required").max(200),
  containerTypeId: z.string().min(1, "Container Type is Required").max(200),
  quantity: z.number().optional(),
  netWeight: z.number().optional(),
  grossWeight: z.number().optional(),
  gracePeriod: z.number().optional(),
  charge20: z.number().optional(),
  charge40: z.number().optional(),
  length: z.number().optional(),
  height: z.number().optional(),
  currencyId: z.string().optional(),
  remark: z.string().optional(),
  sealNumber: z.string().optional(),
  description: z.string().optional(),
  dispatchDate: z.string().optional(),
  containerLoadedDate: z.string().optional(),
});

export const ContainerStatusTemplateSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
  statusId: z.string().min(1, "Status is Required").max(200),
  containerRoleId: z.string().min(1, "Container Role is Required").max(200),
  direction: z.string().min(1, "Direction is Required").max(200),
});

export const ContainerStatusTemplateStatusSchema: z.ZodType<any> = z.object({
  statusId: z.string().min(1, "Status is Required").max(200),
});

export const ContainerStatusTemplateByLIstSchema: z.ZodType<any> = z.object({
  name: z.string().min(1, "Name is Required").max(200),
  operationTypeId: z.string().min(1, "Operation Type is Required").max(200),
  containerRoleId: z.string().min(1, "Role is Required").max(200),
  direction: z.string().min(1, "Direction is Required").max(200),
  details: z
    .array(ContainerStatusTemplateStatusSchema)
    .min(1, "The items array cannot be empty"),
});

export const containerStatusStatusByListSchema: z.ZodType<any> = z.object({
  statusId: z.string().min(1, "Status is Required").max(200),
  value: z.string().max(200),
});
export const containerStatusByLListSchema: z.ZodType<any> = z.object({
  details: z
    .array(containerStatusStatusByListSchema)
    .min(1, "The items array cannot be empty"),
});

export const AdvanceRequestDetailSchema = z.object({
  advanceHeaderId: z.string().max(200).optional(),
  operationId: z.string().min(1, "Operation is Required").max(200),
  jobOrderId: z.string().min(1, "Job Order is Required").max(200),
  customerId: z.string().max(200).optional(),
  amount: z.number().min(1, "Amount must be greater than 0"),
  customer: z.string().max(200).optional(),
  operation: z.string().max(200).optional(),
  jobOrder: z.string().max(200).optional(),
  id: z.string().optional(),
});

export const AdvanceRequestSchema = z.object({
  voucherNumber: z.string().max(200).optional(),
  clientName: z.string().min(1, "Client Name is Required").max(200),
  advanceTypeId: z.string().min(1, "Advance Type is Required").max(200),
  amount: z.number().min(1, "Amount must be greater than 0"),
  purposeId: z.string().min(1, "Purpose is Required").max(200),
  currencyId: z.string().min(1, "Currency is Required").max(200),
  preparedTo: z.string().max(200).optional(),
  date: z.string().min(1, "Date is Required").max(200),
  remark: z.string().max(2000).optional(),
  includeInInvoice: z.boolean().optional(),
  statusId: z.string().max(200).optional(),
  details: z.array(z.any()).optional(),
});

export const AdvanceSettlementSchema = z.object({
  voucherNumber: z.string().max(200).optional(),
  advanceRequestId: z.string().min(1, "Advance Request is Required").max(200),
  amount: z.number().min(1, "Amount must be greater than 0"),
  remark: z.string().max(2000).optional(),
  isSettled: z.boolean().optional(),
  documentUrl: DOCUMENT_SCHEMA.optional(),
  date: z.string().min(1, "Date is Required").max(200),
});

export const InvoiceDetailSchema = z.object({

 serviceDescription: z.string().nullable().optional(),
  unitId: z.string().min(1, "Unit is Required"),
  quantity: z.number().gt(0),
  currencyId: z.string().min(1, "Currency is Required"),
  unitPrice: z.number().gt(0),
  isTaxable: z.boolean(),
  taxExemption: z.number().gte(0),
  remark: z.string().max(2000),
});

export const InvoiceSchema = z.object({
  voucherNumber: z.string().optional(),
  operationTypeId: z.string().min(1, "Operation Type is Required").optional(),
  customerId: z.string().min(1, "Customer is Required").optional(),
  poReferenceNo: z.string().optional(),
  date: z.string().min(1, "Date is Required").optional(),
  customerReferenceNo: z.string().optional(),
  paymentMode: z.string().optional(),
  invoiceType: z.string().optional(),
  fsNo: z.string().optional(),
  finalInvoiceNo: z.string().optional(),
  withHoldingApplied: z.boolean().optional(),
  hbl: z.string().optional(),
  mbl: z.string().optional(),
  carrier: z.string().optional(),
  bl_awb: z.string().optional(),
  etd: z.string().optional(),
  container: z.string().optional(),
  noofContainer: z.string().optional(),
  exchangeRate: z.number().gt(0),
  taxableTotal: z.number().optional(),
  noneTaxableTotal: z.number().optional(),
  vat: z.number().optional(),
  withholding: z.number().optional(),
  net: z.number().optional(),
  advanceAmount: z.number().optional(),
  isSettled: z.boolean().optional(),
  remark: z.string().max(2000).optional(),
  invoicedCustomerId: z.string().nullable().optional(),
  isSendToPOS: z.boolean().nullable().optional(),
  isPosted: z.boolean().nullable().optional(),
  statusId: z.string().optional(),
  status: z.string().optional(),
  id: z.string().optional(),
});

export const OperationExpenseSchema = z.object({
  operationId: z.string().min(1, "Operation is Required").max(200),
  expenseTypeId: z.string().min(1, "Expense Type is Required").max(200),
  value: z.number().min(0, "Value must be positive"),
  remark: z.string().max(2000).optional(),
  advanceSettlementId: z.string().optional(),
});

// ---- Organizational Structure (HRMS §3.1) --------------------------------
export const JobGradeSchema = z.object({
  name: z
    .string()
    .min(2, "Name is Required")
    .max(200)
    .regex(/^[A-Za-z0-9 ]+$/, "Name must be alphanumeric (letters and numbers only)"),
  nameA: z.string().max(200).optional(),
  code: z.string().min(1, "Code is Required").max(50),
});

export const LeaveTypeSchema = z.object({
  code: z.string().min(1, "Code is Required").max(50),
  name: z.string().min(2, "Name is Required").max(200),
  nameA: z.string().max(200).optional(),
});

export const HolidaySchema = z.object({
  date: z.string().min(1, "Date is Required"),
  name: z.string().min(2, "Name is Required").max(200),
  nameA: z.string().max(200).optional(),
});

// Header-only guard; the detail lines are validated in the bespoke JSON save service.
export const LeaveRequestSchema = z.object({
  employeeId: z.string().min(1, "Employee is Required"),
});

export const WorkWeekConfigurationSchema = z.object({
  name: z.string().min(2, "Name is Required").max(150),
});

export const LeaveBalanceSetSchema = z.object({
  employeeId: z.string().min(1, "Employee is Required"),
  leaveTypeId: z.string().min(1, "Leave Type is Required"),
  fiscalYearId: z.string().min(1, "Fiscal Year is Required"),
});

export const AnnualLeaveSettingSchema = z.object({
  fiscalYearId: z.string().min(1, "Fiscal Year is Required"),
  leaveTypeId: z.string().min(1, "Leave Type is Required"),
});

export const SalaryScaleSchema = z.object({
  jobGradeId: z.string().min(1, "Job Grade is Required"),
  stepId: z.string().min(1, "Step is Required"),
  salary: z.coerce.number().min(0, "Salary cannot be negative"),
});
export const JobCategorySchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  code: z.string().min(1, "Code is Required").max(50),
});
export const WorkLocationSchema = z.object({
  code: z.string().min(1, "Code is Required").max(50),
  name: z.string().min(2, "Name is Required").max(200),
  locationType: z.string().min(1, "Location Type is Required").max(30),
});
export const OrganizationUnitSchema = z.object({
  code: z.string().min(1, "Code is Required").max(50),
  name: z.string().min(2, "Name is Required").max(200),
  unitType: z.string().min(1, "Unit Type is Required").max(30),
});
export const PositionSchema = z.object({
  code: z.string().min(1, "Code is Required").max(50),
  positionClassId: z.string().min(1, "Position Class is Required").max(200),
  organizationUnitId: z.string().min(1, "Organization Unit is Required").max(200),
});
export const PositionClassSchema = z.object({
  code: z.string().min(1, "Code is Required").max(50),
  title: z.string().min(2, "Title is Required").max(200),
  salaryScaleId: z.string().min(1, "Salary Scale is Required").max(200),
  jobCategoryId: z.string().min(1, "Job Category is Required").max(200),
});
export const BranchSchema = z.object({
  code: z.string().min(1, "Code is Required").max(50),
  name: z.string().min(2, "Name is Required").max(200),
});
export const EmployeeSchema = z.object({
  employeeNumber: z.string().min(1, "Employee Number is Required").max(50),
  firstName: z.string().min(1, "First Name is Required").max(100),
  fatherName: z.string().max(100).nullish(),
  grandFatherName: z.string().min(1, "Grandfather Name is Required").max(100),
  firstNameA: z.string().max(100).nullish(),
  fatherNameA: z.string().max(100).nullish(),
  grandFatherNameA: z.string().max(100).nullish(),
  gender: z.string().min(1, "Gender is Required"),
  maritalStatus: z.string().min(1, "Marital Status is Required"),
  email: z.string().email("Invalid email address").max(200).nullish().or(z.literal("")),
  salary: z.preprocess(
    (v) => (v === "" || v == null ? undefined : Number(v)),
    z.number("Salary must be a number").min(0, "Salary cannot be negative").optional(),
  ),
  employmentNature: z.string().optional(),
  contractPeriod: z.preprocess(
    (v) => (v === "" || v == null ? undefined : Number(v)),
    z.number("Contract period must be a number").int().positive().optional(),
  ),
  isProbation: z.union([z.boolean(), z.string()]).optional(),
  isManagerial: z.union([z.boolean(), z.string()]).optional(),
  probationEndDate: z.string().nullish(),
})
  // Contract nature requires a contract period.
  .refine((d) => d.employmentNature !== "Contract" || d.contractPeriod != null, {
    message: "Contract Period is required for contract employees.",
    path: ["contractPeriod"],
  })
  // Probation requires an end date.
  .refine((d) => !(d.isProbation === true || d.isProbation === "true") || !!d.probationEndDate, {
    message: "Probation End Date is required when on probation.",
    path: ["probationEndDate"],
  });
export const EmployeeFieldSchema = z.object({
  ownerType: z.string().min(1, "Applies To is Required"),
  name: z
    .string()
    .min(1, "Name is Required")
    .max(100)
    .regex(/^[a-zA-Z][a-zA-Z0-9_]*$/, "Letters, digits and underscores only (start with a letter)"),
  label: z.string().min(1, "Label is Required").max(200),
  dataType: z.string().min(1, "Data Type is Required"),
});
export const EmployeeEducationSchema = z.object({
  employeeId: z.string().min(1),
  educationLevel: z.string().min(1, "Education Level is Required").max(100),
  institution: z.string().min(1, "Institution is Required").max(300),
});
export const EmployeeExperienceSchema = z.object({
  employeeId: z.string().min(1),
  organization: z.string().min(1, "Organization is Required").max(300),
  jobTitle: z.string().min(1, "Job Title is Required").max(200),
});
// Candidate education/experience use the SAME shared form; the owner (candidateId) rides in the URL,
// so these mirror the employee schemas without the employeeId field.
export const CandidateEducationSchema = z.object({
  educationLevel: z.string().min(1, "Education Level is Required").max(100),
  institution: z.string().min(1, "Institution is Required").max(300),
});
export const CandidateExperienceSchema = z.object({
  organization: z.string().min(1, "Organization is Required").max(300),
  jobTitle: z.string().min(1, "Job Title is Required").max(200),
});
export const EmployeeDependentSchema = z.object({
  employeeId: z.string().min(1),
  fullName: z.string().min(1, "Full Name is Required").max(200),
  relationship: z.string().min(1, "Relationship is Required").max(100),
});

export const EmployeeMovementSchema = z.object({
  employeeId: z.string().min(1),
  movementType: z.string().min(1, "Movement type is required"),
  effectiveDate: z.string().min(1, "Effective date is required"),
});

export const DisciplinaryMeasureSchema = z.object({
  employeeId: z.string().min(1),
  violationDate: z.string().min(1, "Violation date is required"),
  violationType: z.string().min(1, "Violation type is required").max(200),
  measureType: z.string().min(1, "Measure is required"),
});

export const EmployeeTerminationSchema = z.object({
  employeeId: z.string().min(1),
  terminationType: z.string().min(1, "Termination type is required"),
  noticeDate: z.string().min(1, "Notice date is required"),
  lastWorkingDate: z.string().min(1, "Last working date is required"),
  reason: z.string().min(1, "Termination reason is required").max(1000),
});

/* ---- Performance Management (HC118–HC147) — Phase A ---- */

export const CompetencyCategorySchema = z.object({
  name: z.string().min(2, "Name is Required").max(150),
});

export const CompetencySchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  competencyCategoryId: z.string().min(1, "Category is Required"),
});

export const AppraisalTemplateSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
});

export const ReviewCycleSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  periodType: z.string().min(1, "Period type is Required"),
  ratingScaleId: z.string().min(1, "Rating scale is Required"),
  startDate: z.string().min(1, "Start date is Required"),
  endDate: z.string().min(1, "End date is Required"),
});

export const OrganizationalObjectiveSchema = z.object({
  title: z.string().min(2, "Title is Required").max(300),
  reviewCycleId: z.string().min(1, "Review cycle is Required"),
});

export const AchievementSchema = z.object({
  employeeId: z.string().min(1, "Employee is Required"),
  title: z.string().min(2, "Title is Required").max(300),
  achievementDate: z.string().min(1, "Date is Required"),
  category: z.string().min(1, "Category is Required"),
});

export const RecognitionBadgeSchema = z.object({
  name: z.string().min(2, "Name is Required").max(150),
});

export const RecognitionSchema = z.object({
  employeeId: z.string().min(1, "Employee is Required"),
  recognitionBadgeId: z.string().min(1, "Badge is Required"),
  citation: z.string().min(2, "Citation is Required").max(1000),
  recognizedOn: z.string().min(1, "Date is Required"),
});

// ===== Reward & Recognition §3.7.4 (HC177–HC186) =====
export const AwardCategorySchema = z.object({
  name: z.string().min(2, "Name is Required").max(150),
});

export const RecognitionProgramSchema = z.object({
  name: z.string().min(2, "Name is Required").max(150),
  period: z.string().min(1, "Period is Required"),
});

export const RewardNominationSchema = z.object({
  nomineeEmployeeId: z.string().min(1, "Nominee is Required"),
  recognitionBadgeId: z.string().min(1, "Award is Required"),
  reason: z.string().min(2, "Reason is Required").max(1000),
});

// ===== Training & Development §3.8 (HC187–HC202) =====
export const TrainingCategorySchema = z.object({
  name: z.string().min(2, "Name is Required").max(150),
});

export const TrainingCourseSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  deliveryMode: z.string().min(1, "Delivery Mode is Required"),
});

export const TrainingSessionSchema = z.object({
  trainingCourseId: z.string().min(1, "Course is Required"),
  startDate: z.string().min(1, "Start Date is Required"),
  endDate: z.string().min(1, "End Date is Required"),
});

// ===== Career Development §3.7.A — Succession Planning =====
export const CriticalPositionSchema = z.object({
  positionId: z.string().min(1, "Position is Required"),
  riskLevel: z.string().min(1, "Risk level is Required"),
});

export const TalentReviewSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  status: z.string().min(1, "Status is Required"),
});

export const SuccessionPlanSchema = z.object({
  criticalPositionId: z.string().min(1, "Critical position is Required"),
  name: z.string().min(2, "Name is Required").max(200),
  horizon: z.string().min(1, "Horizon is Required"),
  status: z.string().min(1, "Status is Required"),
});

// ===== Career Development §3.7.B — Career Path =====
export const CareerPathSchema = z.object({
  name: z.string().min(2, "Name is Required").max(200),
  code: z.string().min(1, "Code is Required").max(50),
});

export const MentorshipSchema = z.object({
  mentorEmployeeId: z.string().min(1, "Mentor is Required"),
  menteeEmployeeId: z.string().min(1, "Mentee is Required"),
  context: z.string().min(1, "Context is Required"),
  status: z.string().min(1, "Status is Required"),
});

export const CareerPathChangeRequestSchema = z.object({
  employeeId: z.string().min(1, "Employee is Required"),
  requestedCareerPathId: z.string().min(1, "Requested career path is Required"),
});

// ===== Compensation §3.10.3 — Insurance =====
export const InsurancePolicySchema = z.object({
  policyNumber: z.string().min(1, "Policy number is Required").max(50),
  insurerName: z.string().min(1, "Insurer is Required").max(200),
});

// ===== Compensation §3.10.2 — Medical =====
export const MedicalProviderSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
});
export const MedicalPlanSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
});
export const MedicalContractSchema = z.object({
  medicalProviderId: z.string().min(1, "Provider is Required"),
});

// ===== Compensation §3.10.4 — Employee Loan =====
export const LoanTypeSchema = z.object({
  name: z.string().min(1, "Name is Required").max(150),
});

// ===== Compensation §3.10.5 — Trip =====
export const PerDiemRateSchema = z.object({
  jobGradeId: z.string().min(1, "Job grade is Required"),
});
export const TripBudgetSchema = z.object({
  fiscalYear: z.string().min(1, "Fiscal year is Required"),
});

// ===== Compensation §3.10.1 — Compensation & Benefit =====
export const AllowanceTypeSchema = z.object({
  name: z.string().min(1, "Name is Required").max(150),
});
export const BenefitPlanSchema = z.object({
  name: z.string().min(1, "Name is Required").max(200),
});
