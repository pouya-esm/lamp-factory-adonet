import { useEffect, useState } from "react";
import { reportsApi } from "../../api/reportsApi.js";
import ReportFilterBar from "../../components/ReportFilterBar.jsx";
import ErrorBanner from "../../components/ui/ErrorBanner.jsx";
import Pagination from "../../components/ui/Pagination.jsx";
import SortableTh from "../../components/ui/SortableTh.jsx";
import usePagedList from "../../hooks/usePagedList.js";
import useSortable from "../../hooks/useSortable.js";

function formatDate(isoString) {
  if (!isoString) return "—";
  return isoString.slice(0, 10);
}

// Positive variance = actual consumption exceeded standard (material waste,
// flagged red). Negative = used less than standard (flagged teal). Zero/null
// is neutral.
function VarianceCell({ value }) {
  if (value == null) return <span className="text-steel-500">—</span>;
  const rounded = Math.round(value * 100) / 100;
  if (rounded > 0)
    return (
      <span className="font-mono font-medium text-stop-500">+{rounded}</span>
    );
  if (rounded < 0)
    return (
      <span className="font-mono font-medium text-line-600">{rounded}</span>
    );
  return <span className="font-mono text-steel-700">0</span>;
}

export default function MaterialConsumptionVariancePage() {
  const [startDate, setStartDate] = useState("");
  const [standardNumber, setStandardNumber] = useState("");
  const [rows, setRows] = useState(null);
  const [error, setError] = useState(null);

  async function runReport() {
    setError(null);
    try {
      const data = await reportsApi.getMaterialConsumptionVariance({
        startDate: startDate || undefined,
        rawMaterialStandardNumber: standardNumber || undefined,
      });
      setRows(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || "Failed to load the report.");
    }
  }

  useEffect(() => {
    runReport();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const { sort, toggleSort, applySort } = useSortable();
  const sortAccessors = {
    doc: (r) => r.productionDocumentID,
    finalProduct: (r) => r.finalProductName,
    date: (r) => r.productionDate,
    produced: (r) => r.producedQuantity,
    rawMaterial: (r) => r.rawMaterialStandardNumber,
    unit: (r) => r.consumptionUnit,
    stdPerUnit: (r) => r.standardConsumptionPerUnit,
    stdTotal: (r) => r.standardTotalConsumption,
    actual: (r) => r.actualConsumption,
    variance: (r) => r.consumptionVariance,
  };
  const sorted = applySort(rows ?? [], sortAccessors);
  const {
    paged,
    page,
    setPage,
    pageSize,
    setPageSize,
    totalPages,
    totalItems,
  } = usePagedList(sorted, [sort]);

  return (
    <div>
      <ReportFilterBar
        startDate={startDate}
        onStartDateChange={setStartDate}
        standardNumber={standardNumber}
        onStandardNumberChange={setStandardNumber}
        onApply={runReport}
        standardNumberLabel="Raw material standard number"
      />

      <ErrorBanner message={error} />

      <div className="rounded-lg border border-panel-300 bg-white overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-panel-100 text-left text-xs uppercase tracking-wider text-steel-500">
                <SortableTh label="Doc #" sortKey="doc" sort={sort} onSort={toggleSort} />
                <SortableTh label="Final Product" sortKey="finalProduct" sort={sort} onSort={toggleSort} />
                <SortableTh label="Date" sortKey="date" sort={sort} onSort={toggleSort} />
                <SortableTh label="Produced" sortKey="produced" sort={sort} onSort={toggleSort} />
                <SortableTh label="Raw Material" sortKey="rawMaterial" sort={sort} onSort={toggleSort} />
                <SortableTh label="Unit" sortKey="unit" sort={sort} onSort={toggleSort} />
                <SortableTh label="Std / Unit" sortKey="stdPerUnit" sort={sort} onSort={toggleSort} className="text-right" />
                <SortableTh label="Std Total" sortKey="stdTotal" sort={sort} onSort={toggleSort} className="text-right" />
                <SortableTh label="Actual" sortKey="actual" sort={sort} onSort={toggleSort} className="text-right" />
                <SortableTh label="Variance" sortKey="variance" sort={sort} onSort={toggleSort} className="text-right" />
              </tr>
            </thead>
            <tbody className="divide-y divide-panel-200">
              {rows === null &&
                [1, 2, 3].map((i) => (
                  <tr key={i}>
                    <td colSpan={10} className="px-4 py-3">
                      <div className="h-4 rounded bg-panel-100 animate-pulse" />
                    </td>
                  </tr>
                ))}

              {rows !== null && paged.length === 0 && (
                <tr>
                  <td
                    colSpan={10}
                    className="px-4 py-10 text-center text-steel-500"
                  >
                    No results for these filters.
                  </td>
                </tr>
              )}

              {paged.map((row, i) => (
                <tr key={i} className="hover:bg-panel-50">
                  <td className="px-4 py-3 font-mono text-steel-800">
                    {row.productionDocumentID}
                  </td>
                  <td className="px-4 py-3 text-steel-700">
                    {row.finalProductName}
                  </td>
                  <td className="px-4 py-3 text-steel-700">
                    {formatDate(row.productionDate)}
                  </td>
                  <td className="px-4 py-3 text-steel-700">
                    {row.producedQuantity ?? "—"}
                  </td>
                  <td className="px-4 py-3 text-steel-700">
                    <span className="font-mono">
                      {row.rawMaterialStandardNumber || "—"}
                    </span>
                    {row.rawMaterialName ? (
                      <span className="text-steel-500">
                        {" "}
                        — {row.rawMaterialName}
                      </span>
                    ) : null}
                  </td>
                  <td className="px-4 py-3 text-steel-700">
                    {row.consumptionUnit ?? "—"}
                  </td>
                  <td className="px-4 py-3 text-right font-mono text-steel-700">
                    {row.standardConsumptionPerUnit ?? "—"}
                  </td>
                  <td className="px-4 py-3 text-right font-mono text-steel-700">
                    {row.standardTotalConsumption ?? "—"}
                  </td>
                  <td className="px-4 py-3 text-right font-mono text-steel-700">
                    {row.actualConsumption ?? "—"}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <VarianceCell value={row.consumptionVariance} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <Pagination
        page={page}
        totalPages={totalPages}
        totalItems={totalItems}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        className="mt-4"
      />

      <div className="mt-3 flex items-center gap-4 text-xs text-steel-500">
        <span className="flex items-center gap-1.5">
          <span className="h-2 w-2 rounded-full bg-stop-500" />
          Over standard (waste)
        </span>
        <span className="flex items-center gap-1.5">
          <span className="h-2 w-2 rounded-full bg-line-500" />
          Under standard
        </span>
      </div>
    </div>
  );
}
