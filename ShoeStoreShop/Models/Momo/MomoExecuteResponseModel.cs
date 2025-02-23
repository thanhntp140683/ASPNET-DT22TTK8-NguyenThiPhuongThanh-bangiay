﻿namespace ShoeStore.Models.Momo
{
    public class MomoExecuteResponseModel
    {
        public string FullName { get; set; }
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string OrderInfo { get; set; }
        public bool IsSuccess { get; set; }
        public int ErrorCode { get; set; }
    }
}
