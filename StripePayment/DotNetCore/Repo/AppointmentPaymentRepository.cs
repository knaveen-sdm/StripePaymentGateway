using HC.Patient.Data;
using HC.Patient.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using HC.Patient.Entity;
using HC.Model;
using System.Linq;
using HC.Repositories;
using System.Data.SqlClient;
using static HC.Common.Enums.CommonEnum;
using HC.Patient.Model;

namespace HC.Patient.Repositories.Repositories
{
    public class AppointmentPaymentRepository : RepositoryBase<AppointmentPayments>, IAppointmentPaymentRepository
    {
        private HCOrganizationContext _context;
        public AppointmentPaymentRepository(HCOrganizationContext context) : base(context)
        {
            this._context = context;
        }
        public IQueryable<T> GetTotalAppointmentRevenue<T>(TokenModel token, int staffId = 0) where T : class, new()
        {
            SqlParameter[] parameters = { new SqlParameter("@organizationId", token.OrganizationID), new SqlParameter("@staffId", staffId) };
            return _context.ExecStoredProcedureListWithOutput<T>(SQLObjects.ADM_GetTotalAppointmentRevenue.ToString(), parameters.Length, parameters).AsQueryable();
        }
        public AppointmentPayments SaveUpdatePayment(AppointmentPayments appointmentPayment, TokenModel tokenModel)
        {
            if (appointmentPayment.Id > 0)
            {
                appointmentPayment.UpdatedBy = tokenModel.UserID;
                appointmentPayment.UpdatedDate = DateTime.UtcNow;
                _context.Update(appointmentPayment);
            }
            else
            {
                appointmentPayment.CreatedBy = tokenModel.UserID;
                appointmentPayment.CreatedDate = DateTime.UtcNow;
                _context.Add(appointmentPayment);
            }
            if (_context.SaveChanges() > 0)
                return appointmentPayment;
            else return null;
        }

        public AppointmentPayments GetAppointmentPaymentsById(int id, TokenModel tokenModel)
        {
            return _context.AppointmentPayments
                .Where(x => x.Id == id && x.OrganizationId == tokenModel.OrganizationID && x.IsActive == true && x.IsDeleted == false)
                .FirstOrDefault();
        }
        public AppointmentPayments GetAppointmentPaymentsByPaymentToken(string paymentToken, TokenModel tokenModel)
        {
            return _context.AppointmentPayments
                .Where(x => x.PaymentToken == paymentToken && x.OrganizationId == tokenModel.OrganizationID && x.IsActive == true && x.IsDeleted == false)
                .FirstOrDefault();
        }
        #region Agency
        public IQueryable<AppointmentPaymentListingModel> GetAppointmentPayments<AppointmentPaymentListingModel>(PaymentFilterModel paymentFilterModel, TokenModel tokenModel) where AppointmentPaymentListingModel : class, new()
        {
            SqlParameter[] parameters = {
                new SqlParameter("@OrganizationId", tokenModel.OrganizationID),
                new SqlParameter("@StaffId", paymentFilterModel.StaffId),
                new SqlParameter("@PatientName", paymentFilterModel.PatientName),
                new SqlParameter("@PayDate", paymentFilterModel.PayDate),
                new SqlParameter("@AppDate", paymentFilterModel.AppDate),
                new SqlParameter("@PageNumber", paymentFilterModel.pageNumber),
                new SqlParameter("@PageSize", paymentFilterModel.pageSize),
                new SqlParameter("@SortColumn", paymentFilterModel.sortColumn),
                new SqlParameter("@SortOrder", paymentFilterModel.sortOrder),
                 new SqlParameter("@Status", paymentFilterModel.Status),
                new SqlParameter("@AppointmentTypeId", paymentFilterModel.AppointmentType),
                new SqlParameter("@RangeStartDate", paymentFilterModel.RangeStartDate),
                new SqlParameter("@RangeEndDate", paymentFilterModel.RangeEndDate),
            };
            return _context.ExecStoredProcedureListWithOutput<AppointmentPaymentListingModel>(SQLObjects.PAY_GetAppointmentPaymentListing.ToString(), parameters.Length, parameters).AsQueryable();
        }
        public IQueryable<AppointmentRefundListingModel> GetAppointmentRefunds<AppointmentRefundListingModel>(RefundFilterModel refundFilterModel, TokenModel tokenModel) where AppointmentRefundListingModel : class, new()
        {
            SqlParameter[] parameters = {
                new SqlParameter("@OrganizationId", tokenModel.OrganizationID),
                new SqlParameter("@StaffId", refundFilterModel.StaffId),
                new SqlParameter("@PatientName", refundFilterModel.PatientName),
                new SqlParameter("@RefundDate", refundFilterModel.RefundDate),
                new SqlParameter("@AppDate", refundFilterModel.AppDate),
                new SqlParameter("@PageNumber", refundFilterModel.pageNumber),
                new SqlParameter("@PageSize", refundFilterModel.pageSize),
                new SqlParameter("@SortColumn", refundFilterModel.sortColumn),
                new SqlParameter("@SortOrder", refundFilterModel.sortOrder)
                
            };
            return _context.ExecStoredProcedureListWithOutput<AppointmentRefundListingModel>(SQLObjects.PAY_GetAppointmentRefundListing.ToString(), parameters.Length, parameters).AsQueryable();
        }

        public IQueryable<AppointmentPaymentListingModel> GetAppointmentPaymentsForReport<AppointmentPaymentListingModel>(PaymentFilterModel paymentFilterModel, TokenModel tokenModel) where AppointmentPaymentListingModel : class, new()
        {
            SqlParameter[] parameters = {
                new SqlParameter("@OrganizationId", tokenModel.OrganizationID),
                new SqlParameter("@StaffId", paymentFilterModel.StaffId),
                new SqlParameter("@PatientName", paymentFilterModel.PatientName),
                new SqlParameter("@PayDate", paymentFilterModel.PayDate),
                new SqlParameter("@AppDate", paymentFilterModel.AppDate),
                new SqlParameter("@PageNumber", paymentFilterModel.pageNumber),
                new SqlParameter("@PageSize", paymentFilterModel.pageSize),
                new SqlParameter("@SortColumn", paymentFilterModel.sortColumn),
                new SqlParameter("@SortOrder", paymentFilterModel.sortOrder),
                 new SqlParameter("@Status", paymentFilterModel.Status),
                new SqlParameter("@AppointmentTypeId", paymentFilterModel.AppointmentType),
                new SqlParameter("@RangeStartDate", paymentFilterModel.RangeStartDate),
                new SqlParameter("@RangeEndDate", paymentFilterModel.RangeEndDate),
            };
            return _context.ExecStoredProcedureListWithOutput<AppointmentPaymentListingModel>(SQLObjects.PAY_GetAppointmentPaymentListingForReport.ToString(), parameters.Length, parameters).AsQueryable();
        }

        #endregion Agency
        #region Client 
        public IQueryable<AppointmentPaymentListingModel> GetClientAppointmentPayments<AppointmentPaymentListingModel>(PaymentFilterModel paymentFilterModel, TokenModel tokenModel) where AppointmentPaymentListingModel : class, new()
        {
            SqlParameter[] parameters = {
                new SqlParameter("@OrganizationId", tokenModel.OrganizationID),
                new SqlParameter("@ClientId", paymentFilterModel.ClientId),
                new SqlParameter("@StaffName", paymentFilterModel.StaffName),
                new SqlParameter("@PayDate", paymentFilterModel.PayDate),
                new SqlParameter("@AppDate", paymentFilterModel.AppDate),
                new SqlParameter("@PageNumber", paymentFilterModel.pageNumber),
                new SqlParameter("@PageSize", paymentFilterModel.pageSize),
                new SqlParameter("@SortColumn", paymentFilterModel.sortColumn),
                new SqlParameter("@SortOrder", paymentFilterModel.sortOrder)
               
            };
            return _context.ExecStoredProcedureListWithOutput<AppointmentPaymentListingModel>(SQLObjects.PAY_GetClientAppointmentPaymentListing.ToString(), parameters.Length, parameters).AsQueryable();
        }
        public IQueryable<AppointmentRefundListingModel> GetClientAppointmentRefunds<AppointmentRefundListingModel>(RefundFilterModel refundFilterModel, TokenModel tokenModel) where AppointmentRefundListingModel : class, new()
        {
            SqlParameter[] parameters = {
                new SqlParameter("@OrganizationId", tokenModel.OrganizationID),
                new SqlParameter("@ClientId", refundFilterModel.ClientId),
                new SqlParameter("@StaffName", refundFilterModel.StaffName),
                new SqlParameter("@RefundDate", refundFilterModel.RefundDate),
                new SqlParameter("@AppDate", refundFilterModel.AppDate),
                new SqlParameter("@PageNumber", refundFilterModel.pageNumber),
                new SqlParameter("@PageSize", refundFilterModel.pageSize),
                new SqlParameter("@SortColumn", refundFilterModel.sortColumn),
                new SqlParameter("@SortOrder", refundFilterModel.sortOrder)
            };
            return _context.ExecStoredProcedureListWithOutput<AppointmentRefundListingModel>(SQLObjects.PAY_GetClientAppointmentRefundListing.ToString(), parameters.Length, parameters).AsQueryable();
        }
        #endregion Client
    }
}
