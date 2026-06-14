using System;
using System.Collections.Generic;

namespace Курсовой_Проект_Трофимова_М.А_ИСПп_1_25в_1_курс.SQL;

public partial class DocumentsLog
{
    public int LogsId { get; set; }

    public string MainTree { get; set; } = null!;

    public DateOnly CreatedAtDay { get; set; }

    public TimeOnly CreatedAtTime { get; set; }

    public bool HasChanged { get; set; }

    public string LogsName { get; set; } = null!;
}
