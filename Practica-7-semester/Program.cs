Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Калькулятор стоимости обучения");
Console.ResetColor();
Console.WriteLine("--------------------------------");
// меню выбора
while (true)
{
    Console.WriteLine("Что хотите сделать?");
    Console.WriteLine();
    Console.WriteLine("Введите 1 чтобы рассчитать стоимость обучения со скидкой");
    Console.WriteLine("Введите 2 чтобы рассчитать сумму кредита на обучение");
    Console.WriteLine("Введите 3 чтобы выйти из программы");
    bool choice = int.TryParse(Console.ReadLine(), out int answerChoice);
    if (!choice)
    {
        PrintError("Надо ввести число");
        continue;
    }
    switch (answerChoice)
    {
        case 1:
            Discount();
            break;
        case 2:
            Credit();
            break;
        case 3:
            return;
        default:
            PrintError("Нет такого пункта в меню");
            break;
    }
}

// расчет скидки на обучение
void Discount()
{
    decimal tuition = CheckDecimal("Введите стоимость обучения", 0, 10_000_000m, "Стоимость обучения не может быть меньше нуля или быть равным нулю а так же больше 10 000 000 руб.");
    decimal tuitionOnDiscount = CheckDecimal("Введите процент скидки", 0, 100, "Процент скидки не может быть ниже 0 или больше 100", false);
    int answerOfGrant = CheckInt("Предоставляется ли вам грант на обучение? (1 - Да / 2 - Нет)", 0, 2, "Ввести нужно 1 или 2");
    decimal grant = 0;
    switch (answerOfGrant)
    {
        case 1:
            grant = CheckDecimal("Введите сумму гранта от нуля до 10 000 000 руб.", 0, 10_000_000, "Сумма гранта не может быть меньше нуля, а так же больше 10 000 000", false);
            break;
        case 2:
            break;
    }
    Console.ForegroundColor = ConsoleColor.Green;
    decimal summ = tuition * (1 - (tuitionOnDiscount / 100));
    Console.WriteLine($"Ваше обучение со скидкой будет стоить - {summ}");
    if (summ != 0)
    {
        if (grant <= summ)
        {
            summ -= grant;
            Console.WriteLine($"Ваше обучение с учетом скидки и гранта будет стоить - {summ} руб.");
        }
        else
        {
            Console.WriteLine($"Ваше обучение с учетом скидки и гранта будет для вас бесплатным");
            summ = 0;
        }
    }
    else
    {
        Console.WriteLine($"С учетом скидки обучение будет для вас бесплатным");
    }

    Console.ResetColor();
    Console.WriteLine();


    int answer = CheckInt("Нужен ли Вам кредит на обучение? (1 - Да / 2 - Нет)", 0, 2, "Ввести нужно 1 или 2");
    switch (answer)
    {
        case 1:
            Credit(summ);
            break;
        case 2:
            return;
    }
}

// рассчет кредита
void Credit(decimal presetSum = 0)
{
    decimal summCredit = presetSum > 0 ? presetSum : CheckDecimal("Введите сумму кредита", 0, 10_000_000m, "Сумма кредита не может быть меньше нуля или быть равным нулю а так же больше 10 000 000");
    decimal percent = CheckDecimal("Введите процент кредитования", 0, 100, "Процент кредитования не может быть ниже 0 или больше 100", false);
    int timeCredit = CheckInt("Введите, на какой период в месяцах берете кредит", 0, 240, "Срок кредита не может быть меньше нуля или быть равным нулю а так же больше 240 месяцев");
    // получаем текущую дату
    DateTime issueDate = DateTime.Today;
    Console.WriteLine($"Дата выдачи кредита: {issueDate:dd.MM.yyyy}");
    // daysToFirst нужен для получения первого платежа по кредиту, интервал от 5 до 60 это разумные границы дней до платежа
    int daysToFirst = CheckInt("Через сколько дней первый платеж? (обычно 20-45)", 5, 60, "Нужно от 5 до 60 дней");
    // аннуитетный платеж формула
    decimal stavka = (percent / 12m) / 100;
    decimal compoundingFactor = 1;
    for (int i = 0; i < timeCredit; i++)
    {
        compoundingFactor *= (1m + stavka);
    }
    decimal basePay = stavka == 0 ? summCredit / timeCredit : summCredit * (stavka + (stavka / (compoundingFactor - 1)));
    // расчет посуточной амортизации
    decimal balance = summCredit;
    decimal totalInterest = 0;
    decimal lastPay = 0;
    DateTime accrualStart = issueDate.AddDays(1);
    DateTime payDate = issueDate.AddDays(daysToFirst);

    for (int k = 0; k < timeCredit; k++)
    {
        decimal interest = AccrueInterest(balance, accrualStart, payDate, percent);
        decimal principal = (k == timeCredit - 1) ? balance : basePay - interest;
        decimal payment = principal + interest;

        totalInterest += interest;
        lastPay = payment;
        balance -= principal;

        accrualStart = payDate;
        payDate = payDate.AddMonths(1);
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Сумма кредита: {summCredit} руб., ставка {percent}% годовых, срок {timeCredit} мес.");
    Console.WriteLine($"Первый платеж: {Math.Round(basePay, 2)} руб. (примерно через {daysToFirst} дней)");
    Console.WriteLine($"Последний платеж: {Math.Round(lastPay, 2)} руб. (скорректирован по фактическому остатку)");
    Console.WriteLine($"Проценты начинаем начислять с {issueDate.AddDays(1):dd.MM.yyyy}");
    Console.WriteLine($"Переплата за весь период: {Math.Round(totalInterest, 2)} руб.");
    Console.WriteLine($"Всего вы заплатите: {summCredit + Math.Round(totalInterest, 2)} руб.");
    Console.ResetColor();
}

// метод для вывода ошибки
void PrintError(string message)
{
    Console.WriteLine("--------------------------------");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ResetColor();
    Console.WriteLine("--------------------------------");
}

// метод для обработки ввода для decimal
decimal CheckDecimal(string message, decimal min, decimal max, string error, bool minInclusive = true)
{
    Console.WriteLine(message);
    while (true)
    {
        if (!decimal.TryParse(Console.ReadLine(), out decimal numbers))
        {
            PrintError("Введенное значение должно быть числом");
            continue;
        }
        bool zeroOk = minInclusive ? numbers <= min : numbers < min;
        if (zeroOk || numbers > max)
        {
            PrintError(error);
            continue;
        }
        return numbers;
    }
}

// метод для обработки ввода для int
int CheckInt(string message, int min, int max, string error, bool minInclusive = true)
{
    Console.WriteLine(message);
    while (true)
    {
        if (!int.TryParse(Console.ReadLine(), out int numbers))
        {
            PrintError("Введенное значение должно быть числом");
            continue;
        }
        bool zeroOk = minInclusive ? numbers <= min : numbers < min;
        if (zeroOk || numbers > max)
        {
            PrintError(error);
            continue;
        }
        return numbers;
    }
}
// метод рассчета процентов на остаток долга посуточно
decimal AccrueInterest(decimal balance, DateTime from, DateTime to, decimal percent)
{
    decimal interest = 0;
    for (DateTime d = from; d < to; d = d.AddDays(1))
        interest += balance * percent / 100m / (DateTime.IsLeapYear(d.Year) ? 366 : 365);
    return interest;
}