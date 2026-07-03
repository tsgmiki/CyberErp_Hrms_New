"use client";

import { useRef } from "react";

import type { ParameterModel } from "@/models";
import PdfViewer from "@/components/common/pdfViewer";
import { Document, Page, View, Text, StyleSheet } from "@react-pdf/renderer";
import PageHeader from "@/components/common/pdfViewer/pageHeader/pageHeader";

function ReportDetail(props: { sourceData: any[]; param?: ParameterModel }) {
  const { sourceData, param } = props;

  const contentRef = useRef<HTMLDivElement>(null);

  // Extract column names and date from sourceData
  const columnNames =
    sourceData && sourceData.length > 0
      ? Object.keys(sourceData[0] as any)
      : [];
  const reportDate =
    sourceData && sourceData.length > 0 ? new Date().toLocaleDateString() : "";

  const styles = StyleSheet.create({
    page: {
      width: "100%",
      padding: 16,
      flexDirection: "column",
    },
    fullWidthSection: {
      width: "100%",
    },
    text: {
      marginBottom: 8,
      fontSize: 10,
    },
    bold: {
      fontWeight: "bold",
    },
  });

  return (
    <>
      <div
        ref={contentRef}
        className=" text-black text-lg bg-white m-[2%] border-solid border-gray-300"
      >
        <PdfViewer>
          <Document title="">
            <Page size="A4" style={styles.page}>
              <PageHeader />
              <View
                style={{
                  justifyContent: "center",
                  padding: 2,
                  fontSize: 10,
                }}
              >
                <Text
                  style={{
                    fontWeight: "bold",
                    fontSize: 14,
                    marginLeft: 4,
                  }}
                >
                  {param?.reportName || "Report"}
                </Text>
                <Text
                  style={{
                    fontSize: 10,
                    marginLeft: 4,
                    marginTop: 4,
                  }}
                >
                  Report Date: {reportDate}
                </Text>
              </View>

              {/* Info Section */}
              <View
                style={{
                  flexDirection: "row",
                  backgroundColor: "#f8fafc", // slate-50
                  margin: 8,
                  padding: 16,
                }}
              >
                {/* Left Column */}
                <View style={{ flex: 1, paddingRight: 8 }}>
                  <Text style={styles.text}>
                    <Text style={styles.bold}>Report Name: </Text>
                    {param?.reportName || "Purchase Order Report"}
                  </Text>
                  <Text style={styles.text}>
                    <Text style={styles.bold}>From Date: </Text>
                    {param?.fromDate || "N/A"}
                  </Text>
                  <Text style={styles.text}>
                    <Text style={styles.bold}>To Date: </Text>
                    {param?.toDate || "N/A"}
                  </Text>
                </View>

                {/* Right Column */}
                <View
                  style={{ flex: 1, alignItems: "flex-end", paddingLeft: 8 }}
                >
                  <Text style={styles.text}>
                    <Text style={styles.bold}>Search Text: </Text>
                    {param?.searchText || "None"}
                  </Text>
                  <Text style={styles.text}>
                    <Text style={styles.bold}>Total Records: </Text>
                    {sourceData?.length || 0}
                  </Text>
                </View>
              </View>

              <View>
                <Text style={{ marginBottom: 8 }}>
                  <Text style={{ fontSize: 10 }}>Details: </Text>
                </Text>
                {sourceData && sourceData.length > 0 && (
                  <View style={{ marginTop: 10 }}>
                    {/* Table Header */}
                    <View
                      style={{
                        flexDirection: "row",
                        borderBottom: "1px solid #000",
                        paddingBottom: 5,
                        marginBottom: 5,
                      }}
                    >
                      {columnNames.map((key: string) => (
                        <Text
                          key={key}
                          style={{
                            flex: 1,
                            fontSize: 9,
                            fontWeight: "bold",
                            textAlign: "center",
                          }}
                        >
                          {key}
                        </Text>
                      ))}
                    </View>
                    {/* Table Rows */}
                    {sourceData.map((item: any, index: number) => (
                      <View
                        key={index}
                        style={{
                          flexDirection: "row",
                          borderBottom: "1px solid #ccc",
                          paddingVertical: 3,
                        }}
                      >
                        {columnNames.map((key: string) => (
                          <Text
                            key={key}
                            style={{
                              flex: 1,
                              fontSize: 8,
                              textAlign: "center",
                            }}
                          >
                            {item[key] || ""}
                          </Text>
                        ))}
                      </View>
                    ))}
                  </View>
                )}
              </View>
            </Page>
          </Document>
        </PdfViewer>
      </div>
    </>
  );
}
export default ReportDetail;
