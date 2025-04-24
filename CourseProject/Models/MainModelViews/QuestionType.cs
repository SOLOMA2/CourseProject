using System.ComponentModel.DataAnnotations;

public enum QuestionType
{
    [Display(Name = "Краткий ответ")]
    Text,

    [Display(Name = "Абзац")]
    Paragraph,

    [Display(Name = "Один из списка")]
    Radio,

    [Display(Name = "Несколько из списка")]
    Checkbox,

    [Display(Name = "Выпадающий список")]
    Dropdown,

    [Display(Name = "Да/Нет")]
    YesNo
}