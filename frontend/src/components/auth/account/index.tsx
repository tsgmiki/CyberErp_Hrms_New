
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import FormNavBar from "@/components/common/formNavBar/formNav";
import Panel from "@/components/common/panel";
import {  useCallback, useState } from "react";
import { v6 as uuid } from "uuid";
//const Password = memo(lazy(() => import("../password/password")));
function Account() {
  const [showDetail, setShowDetail] = useState(false);
  const [, setOption] = useState("");

  const backHandler = useCallback(() => {
    setShowDetail(false);
    setOption("");
  }, []);

  const optionHandler = useCallback((key: string) => {
    setOption(key);
  }, []);

  return (
    <div className={"m-2 "}>
      <FormNavBar
        onList={backHandler}
        hideAdd={true}
        title={"Options"}
        showForm={showDetail}
      ></FormNavBar>

      <div className="md:flex overflow-hidden">
        {
          <Panel>
            <DataTableProvider
              dataTable={{
                isLoading: false,
                hideHeader: true,
                key: uuid(),
                columns: [
                  {
                    name: "name",
                    label: "Category",
                    sort: true,
                    render: (text, record) => (
                      <button onClick={() => optionHandler(record.id)}>
                        {text}
                      </button>
                    ),
                  },
                ],
                data: [
                  { id: "Password", name: "Change Password" },
                  { id: "Company", name: "Company" },
                ] as never,
              }}
            ></DataTableProvider>
          </Panel>
        }
        <div className=" relative flex flex-col flex-1 overflow-y-auto overflow-x-hidden">
          {/* {option == "Password" && <Password />} */}
          </div>
      </div>
    </div>
  );
}
export default Account;
