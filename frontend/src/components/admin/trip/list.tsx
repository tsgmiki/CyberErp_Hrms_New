"use client";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getAllTrips } from "@/services/admin/trip";
import type { TripRequestModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";
import { money, tripStatusBadge } from "./shared";

interface Props {
  onSelect: (id: string) => void;
}

const STATUS_FILTER: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "status",
    label: "Status",
    options: [
      { value: "", label: "All" },
      { value: "Requested", label: "Requested" },
      { value: "Approved", label: "Approved" },
      { value: "InProgress", label: "InProgress" },
      { value: "Completed", label: "Completed" },
      { value: "Settled", label: "Settled" },
      { value: "Rejected", label: "Rejected" },
      { value: "Cancelled", label: "Cancelled" },
    ],
  },
];

function TripList({ onSelect }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({ queryKey: "trips", fetchPage: getAllTrips });

  const columns = useMemo(
    () =>
      [
        {
          name: "tripNumber",
          label: "Trip #",
          sort: true,
          render: (text: string, r: TripRequestModel) => (
            <button type="button" onClick={() => r.id && onSelect(r.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "employeeName", label: "Employee", sort: true },
        { name: "tripType", label: "Type", render: (v: string) => t(v ?? "") },
        { name: "destination", label: "Destination" },
        { name: "dates", label: "Dates", render: (_t: unknown, r: TripRequestModel) => `${r.startDate?.slice(0, 10)} → ${r.endDate?.slice(0, 10)}` },
        {
          name: "advanceAmount",
          label: "Advance",
          render: (_t: unknown, r: TripRequestModel) => <span className="tabular-nums">{money(r.advanceAmount)}{r.advanceDisbursedAt ? " ✓" : ""}</span>,
        },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${tripStatusBadge(text)}`}>{t(text ?? "")}</span>,
        },
      ] as DataTableColumnModel[],
    [onSelect, t],
  );

  return <EntityListShell listKey="trips" listLabel="Business Trips" columns={columns} listFilters={STATUS_FILTER} {...list} />;
}

export default TripList;
