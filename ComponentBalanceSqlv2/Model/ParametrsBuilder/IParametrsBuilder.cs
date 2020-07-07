namespace ComponentBalanceSqlv2.Model.ParametrsBuilder
{
    /// <summary>
    /// Интерфейс установки флага параметра в True или False 
    /// (необходимо отображать параметр для ввода или нет ).
    /// </summary>
    public interface IParametrsBuilder
    {
        void SetIsMonthYear(bool flag);
        void SetIsWorkGuild(bool flag);
        void SetIsWorkGuildOrAll(bool flag);
        void SetIsTmcType(bool flag);
        void SetIsProduct(bool flag);    
    }
}