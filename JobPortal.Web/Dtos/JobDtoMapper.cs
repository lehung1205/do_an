namespace JobPortal.Web.Dtos;

public static class JobDtoMapper
{
    public static CongViecDto ToCongViecDto(this JobDto job) => new()
    {
        id_cong_viec = job.Id,
        id_tuyen_dung = job.EmployerId,
        id_danh_muc = job.CategoryId,
        tieu_de = job.Title,
        mo_ta = job.Description,
        muc_luong = job.Salary,
        dia_diem = job.Location,
        trang_thai_bai_dang = job.PostingStatus,
        ngay_bat_dau = job.StartDate,
        ngay_ket_thuc = job.EndDate,
        ngay_het_han = job.ExpiryDate
    };
}
