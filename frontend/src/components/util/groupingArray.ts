

export function checkObject(arr: any[], record: any, key: any[]) {
  let returnRecord = {} as any;

  arr?.map((a) =>
   { 
    let counter = key?.length;

    key?.map((b) => {
      if (a[b] == record[b]) {
        --counter;
       }
    })
    if(counter==0)
        returnRecord = a;
 
}
  );
   return returnRecord?.id? returnRecord:undefined;
}
