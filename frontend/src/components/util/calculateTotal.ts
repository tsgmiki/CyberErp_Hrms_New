export default function calculateTotal(props: {
  details: { id: string; quantity: number; unitPrice: number }[];
  taxs: { id: string; name: string; rate: number; isAddition: boolean }[];
}) {
  const { details, taxs } = props;
  let total = 0;
  let subTotal = 0;
  let taxSummary = "";
  let commission = 0;

  details?.map((item) => {
    const quantity = item.quantity ?? 0;
    const unitPrice = item.unitPrice ?? 0;
    const itemTotal = quantity * unitPrice;
    total = total + itemTotal;
    subTotal = subTotal + itemTotal;
  });
  taxs?.map((tax) => {
    const rate = tax.rate ?? 0;
    const taxtotal = rate * subTotal;
    const taxamount = tax.isAddition ? taxtotal : -taxtotal;
    total = total + taxamount;
    if (tax.name.indexOf("Commission")>-1) {
      commission = rate * subTotal;
    }
    taxSummary =
      taxSummary + " ; " + tax.name + "=" + taxamount.toLocaleString();
  });
  const totalSummary =
    "Sub Total=" +
    subTotal.toLocaleString() +
    ";" +
    taxSummary +
    ";" +
    " Net Total=" +
    total.toLocaleString();
  return { subTotal, total, totalSummary,commission };
}
