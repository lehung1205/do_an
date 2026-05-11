using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Models;

namespace JobPortal.API;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CongViec, CongViecDto>()
            .ForMember(d => d.id_cong_viec, o => o.MapFrom(s => s.IdCongViec))
            .ForMember(d => d.id_tuyen_dung, o => o.MapFrom(s => s.IdTuyenDung))
            .ForMember(d => d.id_danh_muc, o => o.MapFrom(s => s.IdDanhMuc))
            .ForMember(d => d.tieu_de, o => o.MapFrom(s => s.TieuDe))
            .ForMember(d => d.mo_ta, o => o.MapFrom(s => s.MoTa))
            .ForMember(d => d.muc_luong, o => o.MapFrom(s => s.MucLuong))
            .ForMember(d => d.dia_diem, o => o.MapFrom(s => s.DiaDiem))
            .ForMember(d => d.trang_thai_bai_dang, o => o.MapFrom(s => s.TrangThaiBaiDang))
            .ForMember(d => d.ngay_bat_dau, o => o.MapFrom(s => s.NgayBatDau))
            .ForMember(d => d.ngay_ket_thuc, o => o.MapFrom(s => s.NgayKetThuc))
            .ForMember(d => d.ngay_het_han, o => o.MapFrom(s => s.NgayHetHan));

        CreateMap<CongViecDto, CongViec>()
            .ForMember(d => d.IdCongViec, o => o.Ignore())
            .ForMember(d => d.NhaTuyenDung, o => o.Ignore())
            .ForMember(d => d.DanhMuc, o => o.Ignore())
            .ForMember(d => d.HinhAnhs, o => o.Ignore())
            .ForMember(d => d.UngTuyens, o => o.Ignore())
            .ForMember(d => d.DanhGias, o => o.Ignore())
            .ForMember(d => d.IdTuyenDung, o => o.MapFrom(s => s.id_tuyen_dung))
            .ForMember(d => d.IdDanhMuc, o => o.MapFrom(s => s.id_danh_muc))
            .ForMember(d => d.TieuDe, o => o.MapFrom(s => s.tieu_de))
            .ForMember(d => d.MoTa, o => o.MapFrom(s => s.mo_ta))
            .ForMember(d => d.MucLuong, o => o.MapFrom(s => s.muc_luong))
            .ForMember(d => d.DiaDiem, o => o.MapFrom(s => s.dia_diem))
            .ForMember(d => d.TrangThaiBaiDang, o => o.MapFrom(s => s.trang_thai_bai_dang))
            .ForMember(d => d.NgayBatDau, o => o.MapFrom(s => s.ngay_bat_dau))
            .ForMember(d => d.NgayKetThuc, o => o.MapFrom(s => s.ngay_ket_thuc))
            .ForMember(d => d.NgayHetHan, o => o.MapFrom(s => s.ngay_het_han));
    }
}
