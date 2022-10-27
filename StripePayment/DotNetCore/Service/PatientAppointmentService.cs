using AutoMapper;
using HC.Common;
using HC.Common.Enums;
using HC.Common.HC.Common;
using HC.Common.Model.OrganizationSMTP;
using HC.Common.Services;
using HC.Model;
using HC.Patient.Data;
using HC.Patient.Entity;
using HC.Patient.Model;
using HC.Patient.Model.AppointmentTypes;
using HC.Patient.Model.Common;
using HC.Patient.Model.CouponCodes;
using HC.Patient.Model.CustomMessage;
using HC.Patient.Model.MasterData;
using HC.Patient.Model.NotificationSetting;
using HC.Patient.Model.Organizations;
using HC.Patient.Model.Patient;
using HC.Patient.Model.PatientAppointment;
using HC.Patient.Model.Staff;
using HC.Patient.Repositories.IRepositories;
using HC.Patient.Repositories.IRepositories.Appointment;
using HC.Patient.Repositories.IRepositories.AssignedCouponsClients;
using HC.Patient.Repositories.IRepositories.Locations;
using HC.Patient.Repositories.IRepositories.MasterData;
using HC.Patient.Repositories.IRepositories.Organizations;
using HC.Patient.Repositories.IRepositories.Patient;
using HC.Patient.Repositories.IRepositories.Staff;
using HC.Patient.Repositories.IRepositories.User;
using HC.Patient.Service.IServices;
using HC.Patient.Service.IServices.Chats;
using HC.Patient.Service.IServices.GlobalCodes;
using HC.Patient.Service.IServices.MasterData;
using HC.Patient.Service.IServices.Organizations;
using HC.Patient.Service.IServices.Patient;
using HC.Patient.Service.IServices.PatientAppointment;
using HC.Patient.Service.IServices.Telehealth;
using HC.Patient.Service.Services.Notification;
using HC.Patient.Service.Token.Interfaces;
using HC.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using static HC.Common.Enums.CommonEnum;
using StaffModel = HC.Patient.Model.PatientAppointment.StaffModel;

namespace HC.Patient.Service.PatientApp
{
    public class PatientAppointmentService : BaseService, IPatientAppointmentService
    {
        #region Global Variables
        private readonly IPatientAppointmentRepository _patientAppointmentRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentStaffRepository _appointmentStaffRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IPatientAuthorizationProcedureCPTLinkRepository _patientAuthorizationProcedureCPTLinkRepository;
        private readonly IAppointmentTypeRepository _appointmentTypeRepository;
        private JsonModel response = new JsonModel(new object(), StatusMessage.NotFound, (int)HttpStatusCodes.NotFound);
        private HCOrganizationContext _context;
        private readonly IPatientRepository _patientRepository;
        private readonly IAppointmentAuthorizationRepository _appointmentAuthorizationRepository;
        private readonly IGlobalCodeService _globalCodeService;
        private readonly ILocationRepository _locationRepository;
        private readonly IStaffService _staffService;
        private readonly IPatientService _patientService;
        private readonly IEmailService _emailSender;
        private readonly IHostingEnvironment _env;
        private readonly ITokenService _tokenService;
        private readonly IOrganizationSMTPRepository _organizationSMTPRepository;
        private readonly IEmailWriteService _emailWriteService;
        private readonly IAppointmentPaymentService _appointmentPaymentService;
        private readonly IAppointmentPaymentRepository _appointmentPaymentRepository;

        private readonly IAppointmentPaymentRefundRepository _appointmentPaymentRefundRepository;
        private readonly IMapper _mapper;
        private readonly IOrganizationService _organizationService;
        private readonly ILocationService _locationService;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IChatService _chatService;
        private readonly ITelehealthService _telehealthService;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IAssiggnedCouponsClientsRepository _assiggnedCouponsClientsRepository;

        #endregion
        public PatientAppointmentService(
            INotificationRepository notificationRepository,
            IPatientAppointmentRepository patientAppointmentRepository,
            IAppointmentRepository appointmentRepository,
            IAppointmentStaffRepository appointmentStaffRepository,
            IStaffRepository staffRepository,
            IPatientRepository patientRepository,
            IPatientAuthorizationProcedureCPTLinkRepository patientAuthorizationProcedureCPTLinkRepository,
            IAppointmentAuthorizationRepository appointmentAuthorizationRepository,
            IAppointmentTypeRepository appointmentTypeRepository,
            HCOrganizationContext context,
            IGlobalCodeService globalCodeService,
            ILocationRepository locationRepository,
            IStaffService staffService,
            IPatientService patientService,
            IOrganizationSMTPRepository organizationSMTPRepository,
            ITokenService tokenService,
            IHostingEnvironment env,
            IEmailService emailSender,
            IEmailWriteService emailWriteService,
            IAppointmentPaymentService appointmentPaymentService,
            IAppointmentPaymentRepository appointmentPaymentRepository,
            IMapper mapper,
            IOrganizationService organizationService,
            ILocationService locationService,
            IAppointmentPaymentRefundRepository appointmentPaymentRefundRepository,
            IConfiguration configuration,
            INotificationService notificationService,
            IChatService chatService,
            ITelehealthService telehealthService,
            IUserRepository userRepository,
            IAssiggnedCouponsClientsRepository assiggnedCouponsClientsRepository
            )
        {
            _notificationRepository = notificationRepository;
            _patientAppointmentRepository = patientAppointmentRepository;
            _appointmentRepository = appointmentRepository;
            _appointmentStaffRepository = appointmentStaffRepository;
            _staffRepository = staffRepository;
            _patientAuthorizationProcedureCPTLinkRepository = patientAuthorizationProcedureCPTLinkRepository;
            _context = context;
            _patientRepository = patientRepository;
            _appointmentTypeRepository = appointmentTypeRepository;
            _appointmentAuthorizationRepository = appointmentAuthorizationRepository;
            _globalCodeService = globalCodeService;
            _locationRepository = locationRepository;
            _staffService = staffService;
            _patientService = patientService;
            _organizationSMTPRepository = organizationSMTPRepository;
            _tokenService = tokenService;
            _env = env;
            _emailSender = emailSender;
            _emailWriteService = emailWriteService;
            _appointmentPaymentService = appointmentPaymentService;
            _appointmentPaymentRepository = appointmentPaymentRepository;
            _mapper = mapper;
            _organizationService = organizationService;
            _locationService = locationService;
            _appointmentPaymentRefundRepository = appointmentPaymentRefundRepository;
            _configuration = configuration;
            _notificationService = notificationService;
            _chatService = chatService;
            _telehealthService = telehealthService;
            _userRepository = userRepository;
            _assiggnedCouponsClientsRepository = assiggnedCouponsClientsRepository;

        }
        public List<PatientAppointmentsModel> UpdatePatientAppointment(PatientAppointmentFilter patientAppointmentFilter)
        {
            return _patientAppointmentRepository.UpdatePatientAppointment(patientAppointmentFilter);
        }

        //public List<Model.PatientAppointment.StaffAvailabilityModel> GetStaffAvailability(string StaffID, DateTime FromDate, DateTime ToDate, TokenModel token)
        //{
        //    List<Model.PatientAppointment.StaffAvailabilityModel> availability = _patientAppointmentRepository.GetStaffAvailability(StaffID, FromDate, ToDate);
        //    if (availability != null && availability.Count > 0)
        //    {
        //        availability.ForEach(x => { x.StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(Convert.ToDateTime(x.StartDateTime), token); x.EndDateTime = CommonMethods.ConvertFromUtcTime(Convert.ToDateTime(x.EndDateTime), token); });
        //    }
        //    return availability;
        //}

        //public JsonModel GetPatientAppointmentList(string locationIds, string staffIds, string patientIds, DateTime? fromDate, DateTime? toDate, string patientTags, string staffTags, TokenModel token)
        //{
        //    try
        //    {
        //        List<PatientAppointmentModel> list = new List<PatientAppointmentModel>();
        //        if (!string.IsNullOrEmpty(locationIds) && (!string.IsNullOrEmpty(staffIds) || !string.IsNullOrEmpty(patientIds)))
        //        {
        //            list = _appointmentRepository.GetAppointmentList<PatientAppointmentModel>(locationIds, staffIds, patientIds, fromDate, toDate, patientTags, staffTags, token.OrganizationID).ToList();
        //            list.ForEach(x =>
        //            {
        //                LocationModel locationModal = _locationService.GetLocationOffsets(x.ServiceLocationID, token);//GetLocationOffsets(x.ServiceLocationID);

        //                x.StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(Convert.ToDateTime(x.StartDateTime), locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token);//CommonMethods.ConvertFromUtcTimeWithOffset(x.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
        //                x.EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(Convert.ToDateTime(x.EndDateTime), locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token);//CommonMethods.ConvertFromUtcTimeWithOffset(x.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);

        //                if (!string.IsNullOrEmpty(x.PatientPhotoThumbnailPath)) { x.PatientPhotoThumbnailPath = CommonMethods.CreateImageUrl(token.Request, ImagesPath.PatientThumbPhotos, x.PatientPhotoThumbnailPath); }
        //                x.AppointmentStaffs = !string.IsNullOrEmpty(x.XmlString) ? XDocument.Parse(x.XmlString).Descendants("Child").Select(y => new AppointmentStaffs()
        //                {
        //                    StaffId = Convert.ToInt32(y.Element("StaffId").Value),
        //                    StaffName = y.Element("StaffName").Value,
        //                }).ToList() : new List<AppointmentStaffs>(); x.XmlString = null;
        //                x.InvitedStaffs = !string.IsNullOrEmpty(x.XmlInvitedString) ? XDocument.Parse(x.XmlInvitedString).Descendants("Child").Select(y => new InvitedStaffs()
        //                {
        //                    Name = y.Element("Name").Value,                            Email = y.Element("Email").Value,
        //                }).ToList() : new List<InvitedStaffs>(); x.XmlInvitedString = null;
        //            });
        //        }
        //        return response = new JsonModel()
        //        {
        //            data = list,
        //            Message = StatusMessage.FetchMessage,
        //            StatusCode = (int)HttpStatusCodes.OK
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return response = new JsonModel()
        //        {
        //            data = new object(),
        //            Message = StatusMessage.ServerError,
        //            StatusCode = (int)HttpStatusCodes.InternalServerError,
        //            AppError = ex.Message
        //        };
        //    }
        //}

        public JsonModel GetPatientAppointmentList(string locationIds, string staffIds, string patientIds, DateTime? fromDate, DateTime? toDate, string patientTags, string staffTags, TokenModel token)
        {
            try
            {
                List<PatientAppointmentModel> list = new List<PatientAppointmentModel>();
                if (!string.IsNullOrEmpty(locationIds) && (!string.IsNullOrEmpty(staffIds) || !string.IsNullOrEmpty(patientIds)))
                {
                    list = _appointmentRepository.GetAppointmentList<PatientAppointmentModel>(locationIds, staffIds, patientIds, fromDate, toDate, patientTags, staffTags, token.OrganizationID).ToList();
                    list.ForEach(x =>
                    {
                        LocationModel locationModal = _locationService.GetLocationOffsets(x.ServiceLocationID, token);//GetLocationOffsets(x.ServiceLocationID);

                        x.StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(Convert.ToDateTime(x.StartDateTime), locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token);//CommonMethods.ConvertFromUtcTimeWithOffset(x.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                        x.EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(Convert.ToDateTime(x.EndDateTime), locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token);//CommonMethods.ConvertFromUtcTimeWithOffset(x.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);

                        //r t = _context.SymptomatePatientReport.Where(s => s.PatientID == x.PatientAppointmentId).Select(s => s.PatientID);
                        var result= _context.SymptomatePatientReport.Where(s => s.PatientID == x.PatientAppointmentId).Select(s => s.Id).FirstOrDefault(); 
                        if (result != 0)
                        {
                            x.IsSymptomateReportExist = true;
                            x.ReportId = result;
                        }

                        if (!string.IsNullOrEmpty(x.PatientPhotoThumbnailPath)) { x.PatientPhotoThumbnailPath = CommonMethods.CreateImageUrl(token.Request, ImagesPath.PatientThumbPhotos, x.PatientPhotoThumbnailPath); }
                        x.AppointmentStaffs = !string.IsNullOrEmpty(x.XmlString) ? XDocument.Parse(x.XmlString).Descendants("Child").Select(y => new AppointmentStaffs()
                        {
                            StaffId = Convert.ToInt32(y.Element("StaffId").Value),
                            StaffName = y.Element("StaffName").Value,
                        }).ToList() : new List<AppointmentStaffs>(); x.XmlString = null;
                        x.InvitedStaffs = !string.IsNullOrEmpty(x.XmlInvitedString) ? XDocument.Parse(x.XmlInvitedString).Descendants("Child").Select(y => new InvitedStaffs()
                        {
                            Name = y.Element("Name").Value,
                            Email = y.Element("Email").Value,
                        }).ToList() : new List<InvitedStaffs>(); x.XmlInvitedString = null;
                    });
                }
                return response = new JsonModel()
                {
                    data = list,
                    Message = StatusMessage.FetchMessage,
                    StatusCode = (int)HttpStatusCodes.OK
                };
            }
            catch (Exception ex)
            {
                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.ServerError,
                    StatusCode = (int)HttpStatusCodes.InternalServerError,
                    AppError = ex.Message
                };
            }
        }

        public JsonModel GetPatientAppointmentListForDashboard(string locationIds, string staffIds, string patientIds, DateTime? fromDate, DateTime? toDate, string patientTags, string staffTags, TokenModel token)
        {
            try
            {
                List<PatientAppointmentModel> list = new List<PatientAppointmentModel>();
                var user = _tokenService.GetUserById(token);
                var isValid = false;
                if (user != null)
                {
                    if (user.UserRoles.RoleName == OrganizationRoles.Admin.ToString())
                        isValid = true;

                    else if (!string.IsNullOrEmpty(locationIds) && (!string.IsNullOrEmpty(staffIds) || !string.IsNullOrEmpty(patientIds)))
                        isValid = true;
                }
                if (isValid)
                    list = _appointmentRepository.GetAppointmentList<PatientAppointmentModel>(locationIds, staffIds, patientIds, fromDate, toDate, patientTags, staffTags, token.OrganizationID).ToList();
                return response = new JsonModel()
                {
                    data = list,
                    Message = StatusMessage.FetchMessage,
                    StatusCode = (int)HttpStatusCodes.OK
                };
            }
            catch (Exception ex)
            {
                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.ServerError,
                    StatusCode = (int)HttpStatusCodes.InternalServerError,
                    AppError = ex.Message
                };
            }
        }

        //public JsonModel SaveAppointment(PatientAppointmentModel patientAppointmentModel, List<PatientAppointmentModel> patientAppointmentList, bool isAdmin, TokenModel tokenModel)
        //{
        //    ///Sunny Bhardwaj - Move the whole code for this function to sql queries later as it will slow down the process in bulk data
        //    int[] aptIds = { };  //This has been added to remove transaction as it was giving conflicts in context and dbcommand transaction
        //    string action = "";//add,update
        //    PatientAppointment patientAppointment = null;
        //    List<PatientAppointment> patientAptList = null;
        //    List<AppointmentStaff> appointmentStaffList = new List<AppointmentStaff>();
        //    AppointmentStaff appointmentStaffs = null;
        //    List<AppointmentStaff> appointmentStaffNewList = new List<AppointmentStaff>();
        //    List<AppointmentAuthModel> list = null;
        //    //List<AppointmentAuthModel> updateList = null;

        //    bool isDelete = false;

        //    #region  Check Authorization whether insert case or update case

        //    if (_patientRepository.CheckAuthorizationSetting() == true && patientAppointmentModel.PatientID != null && patientAppointmentModel.PatientID > 0)
        //    {
        //        if (patientAppointmentModel.PatientAppointmentId == 0)
        //        {

        //            list = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)patientAppointmentModel.PatientID, patientAppointmentModel.AppointmentTypeID, patientAppointmentModel.StartDateTime, patientAppointmentModel.EndDateTime, InsurancePlanType.Primary.ToString(), patientAppointmentModel.PatientAppointmentId, isAdmin, patientAppointmentModel.PatientInsuranceId, patientAppointmentModel.AuthorizationId).Where(x => x.AuthProcedureCPTLinkId != null && x.AuthProcedureCPTLinkId > 0).ToList();
        //            if (list != null && list.Count > 0 && list.First().AuthorizationMessage.ToLower() != "valid")
        //                return new JsonModel()
        //                {
        //                    data = new object(),
        //                    Message = list.First().AuthorizationMessage,
        //                    StatusCode = (int)HttpStatusCodes.UnprocessedEntity
        //                };
        //        }
        //        else
        //        {
        //            patientAppointment = _appointmentRepository.Get(x => x.Id == patientAppointmentModel.PatientAppointmentId);
        //            if (patientAppointment != null)
        //            {
        //                if (patientAppointment.AppointmentTypeID != patientAppointmentModel.AppointmentTypeID || patientAppointment.EndDateTime.Subtract(patientAppointment.StartDateTime).TotalMinutes != patientAppointmentModel.EndDateTime.Subtract(patientAppointmentModel.StartDateTime).TotalMinutes)
        //                {
        //                    list = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)patientAppointmentModel.PatientID, patientAppointmentModel.AppointmentTypeID, patientAppointmentModel.StartDateTime, patientAppointmentModel.EndDateTime, InsurancePlanType.Primary.ToString(), patientAppointmentModel.PatientAppointmentId, isAdmin, patientAppointmentModel.PatientInsuranceId, patientAppointmentModel.AuthorizationId).Where(x => x.AuthProcedureCPTLinkId != null && x.AuthProcedureCPTLinkId > 0).ToList();
        //                    if (list != null && list.Count > 0 && list.First().AuthorizationMessage.ToLower() != "valid")
        //                        return new JsonModel()
        //                        {
        //                            data = new object(),
        //                            Message = list.First().AuthorizationMessage,
        //                            StatusCode = (int)HttpStatusCodes.UnprocessedEntity
        //                        };
        //                    else
        //                    {
        //                        isDelete = true;
        //                        //updateList = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)patientAppointment.PatientID, patientAppointment.AppointmentTypeID, patientAppointment.StartDateTime, patientAppointment.EndDateTime, InsurancePlanType.Primary.ToString(), patientAppointmentModel.PatientAppointmentId).Where(x => x.AuthProcedureCPTLinkId != null && x.AuthProcedureCPTLinkId > 0).ToList();
        //                        patientAppointment = null;
        //                    }
        //                }
        //            }
        //            //}
        //        }
        //    }
        //    #endregion

        //    //using (var transaction = _context.Database.BeginTransaction())
        //    //{
        //    try
        //    {
        //        if (patientAppointmentModel.PatientAppointmentId == 0)
        //        {
        //            action = "add";
        //            patientAppointment = new PatientAppointment();
        //            AutoMapper.Mapper.Map(patientAppointmentModel, patientAppointment);
        //            patientAppointment.OrganizationID = tokenModel.OrganizationID;
        //            patientAppointment.IsActive = true;
        //            patientAppointment.IsDeleted = false;
        //            patientAppointment.CreatedBy = tokenModel.UserID;
        //            patientAppointment.CreatedDate = DateTime.UtcNow;
        //            patientAppointment.ParentAppointmentID = null;
        //            patientAppointment.AppointmentTypeID = null;
        //            patientAppointment.IsDirectService = patientAppointmentModel.IsDirectService;


        //            patientAppointment.IsExcludedFromMileage = patientAppointmentModel.IsExcludedFromMileage;
        //            patientAppointment.DriveTime = patientAppointmentModel.DriveTime;
        //            patientAppointment.Mileage = patientAppointmentModel.Mileage;
        //            patientAppointment.Offset = patientAppointmentModel.OffSet;
        //            patientAppointment.StatusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, AppointmentStatus.APPROVED, tokenModel);

        //            patientAppointment.RecurrenceRule = !string.IsNullOrEmpty(patientAppointmentModel.RecurrenceRule) ? patientAppointmentModel.RecurrenceRule : null;
        //            if (patientAppointmentList != null && patientAppointmentList.Count > 0)
        //                patientAppointment.IsRecurrence = true;
        //            else patientAppointment.IsRecurrence = false;
        //            _appointmentRepository.Create(patientAppointment);
        //            _appointmentRepository.SaveChanges();
        //            aptIds.Append(patientAppointment.Id);
        //            if (list != null && list.Count > 0)
        //                UpdateScheduledUnits(tokenModel, list, "add", patientAppointment.Id);
        //            appointmentStaffList = patientAppointmentModel.AppointmentStaffs.Select(x => new AppointmentStaff() { StaffID = x.StaffId, PatientAppointmentID = patientAppointment.Id, CreatedBy = tokenModel.UserID, CreatedDate = DateTime.UtcNow, IsActive = true, IsDeleted = false }).ToList();
        //            _appointmentStaffRepository.Create(appointmentStaffList.ToArray());
        //            _appointmentStaffRepository.SaveChanges();
        //            bool isBillableService = _appointmentTypeRepository.GetByID(patientAppointment.AppointmentTypeID).IsBillAble;
        //            if (!string.IsNullOrEmpty(patientAppointmentModel.RecurrenceRule) && patientAppointmentList != null && patientAppointmentList.Count > 0)
        //            {
        //                patientAptList = new List<PatientAppointment>();
        //                AutoMapper.Mapper.Map(patientAppointmentList, patientAptList);
        //                patientAptList.ForEach(x =>
        //                {
        //                    if (isBillableService)
        //                        list = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)x.PatientID, (int)x.AppointmentTypeID, x.StartDateTime, x.EndDateTime, InsurancePlanType.Primary.ToString(), null, isAdmin, x.PatientInsuranceId, x.AuthorizationId).Where(a => a.AuthProcedureCPTLinkId != null && a.AuthProcedureCPTLinkId > 0).ToList();
        //                    if (isBillableService == false || (list != null && list.Count > 0 && list.First().AuthorizationMessage.ToLower() == "valid"))
        //                    {
        //                        x.OrganizationID = tokenModel.OrganizationID;
        //                        x.CreatedDate = DateTime.UtcNow;
        //                        x.IsDeleted = false;
        //                        x.IsActive = true;
        //                        x.ParentAppointmentID = patientAppointment.Id;
        //                        x.RecurrenceRule = null;
        //                        x.IsRecurrence = true;
        //                        x.StatusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, AppointmentStatus.APPROVED, tokenModel);
        //                        _appointmentRepository.Create(x);
        //                        _appointmentRepository.SaveChanges();
        //                        aptIds.Append(x.Id);
        //                        appointmentStaffList = new List<AppointmentStaff>();
        //                        appointmentStaffList.AddRange(patientAppointmentModel.AppointmentStaffs.Select(a => new AppointmentStaff() { StaffID = a.StaffId, PatientAppointmentID = x.Id, CreatedBy = tokenModel.UserID, CreatedDate = DateTime.UtcNow, IsActive = true, IsDeleted = false }).ToList());
        //                        _appointmentStaffRepository.Create(appointmentStaffList.ToArray());
        //                        _appointmentStaffRepository.SaveChanges();
        //                        if (isBillableService)
        //                            UpdateScheduledUnits(tokenModel, list, "add", x.Id);
        //                    }
        //                    else
        //                    {

        //                    }
        //                });
        //                //_appointmentRepository.Create(patientAptList.ToArray());
        //                //_appointmentRepository.SaveChanges();
        //                //appointmentStaffList = new List<AppointmentStaff>();
        //                //foreach (PatientAppointment patientApt in patientAptList)
        //                //{
        //                //    appointmentStaffList.AddRange(patientAppointmentModel.AppointmentStaffs.Select(x => new AppointmentStaff() { StaffID = x.StaffId, PatientAppointmentID = patientApt.Id, CreatedBy = tokenModel.UserID, CreatedDate = DateTime.UtcNow, IsActive = true, IsDeleted = false }).ToList());
        //                //}
        //                //_appointmentStaffRepository.Create(appointmentStaffList.ToArray());
        //                //_appointmentStaffRepository.SaveChanges();
        //            }
        //            response = new JsonModel()
        //            {
        //                Message = StatusMessage.AddAppointment,
        //                StatusCode = (int)HttpStatusCodes.OK,
        //                data = new object()
        //            };
        //        }
        //        else
        //        {
        //            using (var transaction = _context.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    patientAppointment = _appointmentRepository.Get(x => x.Id == patientAppointmentModel.PatientAppointmentId && x.IsActive == true && x.IsDeleted == false);
        //                    if (!ReferenceEquals(patientAppointment, null))
        //                    {
        //                        action = "update";
        //                        patientAppointment.StartDateTime = patientAppointmentModel.StartDateTime;
        //                        patientAppointment.EndDateTime = patientAppointmentModel.EndDateTime;
        //                        //patientAppointment.AppointmentTypeID = patientAppointmentModel.AppointmentTypeID;
        //                        patientAppointment.AppointmentTypeID = null;
        //                        patientAppointment.Notes = patientAppointmentModel.Notes;
        //                        patientAppointment.PatientAddressID = patientAppointmentModel.PatientAddressID;
        //                        patientAppointment.ServiceLocationID = patientAppointmentModel.ServiceLocationID;
        //                        patientAppointment.OfficeAddressID = patientAppointmentModel.OfficeAddressID;
        //                        patientAppointment.CustomAddress = patientAppointmentModel.CustomAddress;
        //                        patientAppointment.CustomAddressID = patientAppointmentModel.CustomAddressID;
        //                        patientAppointment.Longitude = patientAppointmentModel.Longitude;
        //                        patientAppointment.Latitude = patientAppointmentModel.Latitude;
        //                        patientAppointment.ApartmentNumber = patientAppointmentModel.ApartmentNumber;
        //                        patientAppointment.IsDirectService = patientAppointmentModel.IsDirectService;
        //                        patientAppointment.UpdatedBy = tokenModel.UserID;
        //                        patientAppointment.UpdatedDate = DateTime.UtcNow;
        //                        patientAppointment.IsTelehealthAppointment = patientAppointmentModel.IsTelehealthAppointment;
        //                        patientAppointment.Offset = patientAppointmentModel.OffSet;

        //                        patientAppointment.IsExcludedFromMileage = patientAppointmentModel.IsExcludedFromMileage;
        //                        patientAppointment.DriveTime = patientAppointmentModel.DriveTime;
        //                        patientAppointment.Mileage = patientAppointmentModel.Mileage;
        //                        patientAppointment.PatientInsuranceId = patientAppointmentModel.PatientInsuranceId;
        //                        patientAppointment.AuthorizationId = patientAppointmentModel.AuthorizationId;
        //                        _appointmentRepository.Update(patientAppointment);
        //                        _appointmentRepository.SaveChanges();

        //                        if (isDelete)
        //                            UpdateScheduledUnits(tokenModel, null, "delete", patientAppointment.Id);
        //                        if (list != null && list.Count > 0)
        //                            UpdateScheduledUnits(tokenModel, list, "add", patientAppointment.Id);
        //                        appointmentStaffList = _appointmentStaffRepository.GetAll(x => x.PatientAppointmentID == patientAppointmentModel.PatientAppointmentId && x.IsActive == true && x.IsDeleted == false).ToList();
        //                        foreach (AppointmentStaffs aptStaff in patientAppointmentModel.AppointmentStaffs)
        //                        {
        //                            appointmentStaffs = appointmentStaffList.Find(x => x.PatientAppointmentID == patientAppointmentModel.PatientAppointmentId && x.StaffID == aptStaff.StaffId);
        //                            if (appointmentStaffs != null)
        //                            {
        //                                if (aptStaff.IsDeleted == true)
        //                                {
        //                                    appointmentStaffs.IsDeleted = aptStaff.IsDeleted;
        //                                    appointmentStaffs.DeletedBy = tokenModel.UserID;
        //                                    appointmentStaffs.DeletedDate = DateTime.UtcNow;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                appointmentStaffs = new AppointmentStaff();
        //                                appointmentStaffs.PatientAppointmentID = patientAppointmentModel.PatientAppointmentId;
        //                                appointmentStaffs.StaffID = aptStaff.StaffId;
        //                                appointmentStaffs.CreatedBy = tokenModel.UserID;
        //                                appointmentStaffs.CreatedDate = DateTime.UtcNow;
        //                                appointmentStaffNewList.Add(appointmentStaffs);
        //                            }
        //                        }
        //                        if (appointmentStaffList.FindAll(x => x.IsDeleted == true).Count > 0)
        //                            _appointmentStaffRepository.Update(appointmentStaffList.ToArray());
        //                        if (appointmentStaffNewList != null && appointmentStaffNewList.Count > 0)
        //                            _appointmentStaffRepository.Create(appointmentStaffNewList.ToArray());

        //                        _appointmentStaffRepository.SaveChanges();
        //                        response = new JsonModel() { Message = StatusMessage.UpdateAppointment };
        //                    }
        //                    else
        //                        response = new JsonModel() { Message = StatusMessage.AppointmentNotExists };

        //                    response.data = new object();
        //                    response.StatusCode = (int)HttpStatusCodes.OK;
        //                    transaction.Commit();
        //                }
        //                catch(Exception ex)
        //                {
        //                    transaction.Rollback();
        //                    return response = new JsonModel()
        //                    {
        //                        data = new object(),
        //                        Message = StatusMessage.ServerError,
        //                        StatusCode = (int)HttpStatusCodes.InternalServerError
        //                    };
        //                }
        //            }
        //        }

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        //This code has been added on 25April to avoid trnasaction conflicts in Context and DbCommand.
        //        //It should be removed once that issue will be resolved
        //        if (action != "" && action == "add")
        //        {
        //            List<AppointmentAuthorization> authList = _appointmentAuthorizationRepository.GetAll(x => aptIds.Contains(x.AppointmentId)).ToList();
        //            List<AppointmentStaff> aptStaff = _appointmentStaffRepository.GetAll(x => aptIds.Contains(x.PatientAppointmentID)).ToList();
        //            List<PatientAppointment> aptList = _appointmentRepository.GetAll(x => aptIds.Contains(x.Id)).ToList();
        //            if (aptStaff != null && aptStaff.Count > 0)
        //                _appointmentStaffRepository.Delete(aptStaff.ToArray());
        //            if (authList != null && authList.Count > 0)
        //                _appointmentAuthorizationRepository.Delete(authList.ToArray());
        //            if (aptList != null && aptList.Count > 0)
        //                _appointmentRepository.Delete(aptList.ToArray());
        //            _appointmentRepository.SaveChanges();
        //        }
        //        return response = new JsonModel()
        //        {
        //            data = new object(),
        //            Message = StatusMessage.ServerError,
        //            StatusCode = (int)HttpStatusCodes.InternalServerError,
        //            AppError = ex.Message
        //        };
        //    }
        //    //}
        //}


        public JsonModel SaveAppointment(PatientAppointmentModel patientAppointmentModel, List<PatientAppointmentModel> patientAppointmentList, bool isAdmin, TokenModel tokenModel)
        {
            ///Sunny Bhardwaj - Move the whole code for this function to sql queries later as it will slow down the process in bulk data
            int[] aptIds = { };  //This has been added to remove transaction as it was giving conflicts in context and dbcommand transaction
            string action = "";//add,update
            PatientAppointment patientAppointment = null;
            List<PatientAppointment> patientAptList = null;
            List<AppointmentStaff> appointmentStaffList = new List<AppointmentStaff>();
            AppointmentStaff appointmentStaffs = null;
            List<AppointmentStaff> appointmentStaffNewList = new List<AppointmentStaff>();
            List<AppointmentAuthModel> list = null;
            //List<AppointmentAuthModel> updateList = null;

            bool isDelete = false;

            bool IsSymptomateReportExist = false;
            int ReportId = 0;

            #region  Check Authorization whether insert case or update case

            if (_patientRepository.CheckAuthorizationSetting() == true && patientAppointmentModel.PatientID != null && patientAppointmentModel.PatientID > 0)
            {
                if (patientAppointmentModel.PatientAppointmentId == 0)
                {

                    list = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)patientAppointmentModel.PatientID, patientAppointmentModel.AppointmentTypeID, patientAppointmentModel.StartDateTime, patientAppointmentModel.EndDateTime, InsurancePlanType.Primary.ToString(), patientAppointmentModel.PatientAppointmentId, isAdmin, patientAppointmentModel.PatientInsuranceId, patientAppointmentModel.AuthorizationId).Where(x => x.AuthProcedureCPTLinkId != null && x.AuthProcedureCPTLinkId > 0).ToList();
                    if (list != null && list.Count > 0 && list.First().AuthorizationMessage.ToLower() != "valid")
                        return new JsonModel()
                        {
                            data = new object(),
                            Message = list.First().AuthorizationMessage,
                            StatusCode = (int)HttpStatusCodes.UnprocessedEntity
                        };
                }
                else
                {
                    patientAppointment = _appointmentRepository.Get(x => x.Id == patientAppointmentModel.PatientAppointmentId);
                    if (patientAppointment != null)
                    {
                        if (patientAppointment.AppointmentTypeID != patientAppointmentModel.AppointmentTypeID || patientAppointment.EndDateTime.Subtract(patientAppointment.StartDateTime).TotalMinutes != patientAppointmentModel.EndDateTime.Subtract(patientAppointmentModel.StartDateTime).TotalMinutes)
                        {
                            list = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)patientAppointmentModel.PatientID, patientAppointmentModel.AppointmentTypeID, patientAppointmentModel.StartDateTime, patientAppointmentModel.EndDateTime, InsurancePlanType.Primary.ToString(), patientAppointmentModel.PatientAppointmentId, isAdmin, patientAppointmentModel.PatientInsuranceId, patientAppointmentModel.AuthorizationId).Where(x => x.AuthProcedureCPTLinkId != null && x.AuthProcedureCPTLinkId > 0).ToList();
                            if (list != null && list.Count > 0 && list.First().AuthorizationMessage.ToLower() != "valid")
                                return new JsonModel()
                                {
                                    data = new object(),
                                    Message = list.First().AuthorizationMessage,
                                    StatusCode = (int)HttpStatusCodes.UnprocessedEntity
                                };
                            else
                            {
                                isDelete = true;
                                //updateList = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)patientAppointment.PatientID, patientAppointment.AppointmentTypeID, patientAppointment.StartDateTime, patientAppointment.EndDateTime, InsurancePlanType.Primary.ToString(), patientAppointmentModel.PatientAppointmentId).Where(x => x.AuthProcedureCPTLinkId != null && x.AuthProcedureCPTLinkId > 0).ToList();
                                patientAppointment = null;
                            }
                        }
                    }
                    //}
                }
            }
            #endregion

            //using (var transaction = _context.Database.BeginTransaction())
            //{
            try
            {
                if (patientAppointmentModel.PatientAppointmentId == 0)
                {
                    action = "add";
                    patientAppointment = new PatientAppointment();
                    AutoMapper.Mapper.Map(patientAppointmentModel, patientAppointment);
                    patientAppointment.OrganizationID = tokenModel.OrganizationID;
                    patientAppointment.IsActive = true;
                    patientAppointment.IsDeleted = false;
                    patientAppointment.CreatedBy = tokenModel.UserID;
                    patientAppointment.CreatedDate = DateTime.UtcNow;
                    patientAppointment.ParentAppointmentID = null;
                    patientAppointment.AppointmentTypeID = null;
                    patientAppointment.IsTelehealthAppointment = true;
                    patientAppointment.IsDirectService = true;
                    patientAppointment.IsExcludedFromMileage = true;
                    patientAppointment.DriveTime = patientAppointmentModel.DriveTime;
                    patientAppointment.Mileage = patientAppointmentModel.Mileage;
                    patientAppointment.Offset = patientAppointmentModel.OffSet;
                    patientAppointment.StatusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, AppointmentStatus.APPROVED, tokenModel);

                    patientAppointment.RecurrenceRule = !string.IsNullOrEmpty(patientAppointmentModel.RecurrenceRule) ? patientAppointmentModel.RecurrenceRule : null;
                    if (patientAppointmentList != null && patientAppointmentList.Count > 0)
                        patientAppointment.IsRecurrence = true;
                    else patientAppointment.IsRecurrence = false;
                    _appointmentRepository.Create(patientAppointment);
                    _appointmentRepository.SaveChanges();
                    aptIds.Append(patientAppointment.Id);
                    if (list != null && list.Count > 0)
                        UpdateScheduledUnits(tokenModel, list, "add", patientAppointment.Id);
                    appointmentStaffList = patientAppointmentModel.AppointmentStaffs.Select(x => new AppointmentStaff() { StaffID = x.StaffId, PatientAppointmentID = patientAppointment.Id, CreatedBy = tokenModel.UserID, CreatedDate = DateTime.UtcNow, IsActive = true, IsDeleted = false }).ToList();
                    _appointmentStaffRepository.Create(appointmentStaffList.ToArray());
                    _appointmentStaffRepository.SaveChanges();
                    bool isBillableService = _appointmentTypeRepository.GetByID(patientAppointment.AppointmentTypeID).IsBillAble;
                    if (!string.IsNullOrEmpty(patientAppointmentModel.RecurrenceRule) && patientAppointmentList != null && patientAppointmentList.Count > 0)
                    {
                        patientAptList = new List<PatientAppointment>();
                        AutoMapper.Mapper.Map(patientAppointmentList, patientAptList);
                        patientAptList.ForEach(x =>
                        {
                            if (isBillableService)
                                list = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)x.PatientID, (int)x.AppointmentTypeID, x.StartDateTime, x.EndDateTime, InsurancePlanType.Primary.ToString(), null, isAdmin, x.PatientInsuranceId, x.AuthorizationId).Where(a => a.AuthProcedureCPTLinkId != null && a.AuthProcedureCPTLinkId > 0).ToList();
                            if (isBillableService == false || (list != null && list.Count > 0 && list.First().AuthorizationMessage.ToLower() == "valid"))
                            {
                                x.OrganizationID = tokenModel.OrganizationID;
                                x.CreatedDate = DateTime.UtcNow;
                                x.IsDeleted = false;
                                x.IsActive = true;
                                x.ParentAppointmentID = patientAppointment.Id;
                                x.RecurrenceRule = null;
                                x.IsRecurrence = true;
                                x.StatusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, AppointmentStatus.APPROVED, tokenModel);
                                _appointmentRepository.Create(x);
                                _appointmentRepository.SaveChanges();
                                aptIds.Append(x.Id);
                                appointmentStaffList = new List<AppointmentStaff>();
                                appointmentStaffList.AddRange(patientAppointmentModel.AppointmentStaffs.Select(a => new AppointmentStaff() { StaffID = a.StaffId, PatientAppointmentID = x.Id, CreatedBy = tokenModel.UserID, CreatedDate = DateTime.UtcNow, IsActive = true, IsDeleted = false }).ToList());
                                _appointmentStaffRepository.Create(appointmentStaffList.ToArray());
                                _appointmentStaffRepository.SaveChanges();
                                if (isBillableService)
                                    UpdateScheduledUnits(tokenModel, list, "add", x.Id);
                            }
                            else
                            {

                            }
                        });
                        //_appointmentRepository.Create(patientAptList.ToArray());
                        //_appointmentRepository.SaveChanges();
                        //appointmentStaffList = new List<AppointmentStaff>();
                        //foreach (PatientAppointment patientApt in patientAptList)
                        //{
                        //    appointmentStaffList.AddRange(patientAppointmentModel.AppointmentStaffs.Select(x => new AppointmentStaff() { StaffID = x.StaffId, PatientAppointmentID = patientApt.Id, CreatedBy = tokenModel.UserID, CreatedDate = DateTime.UtcNow, IsActive = true, IsDeleted = false }).ToList());
                        //}
                        //_appointmentStaffRepository.Create(appointmentStaffList.ToArray());
                        //_appointmentStaffRepository.SaveChanges();
                    }
                    response = new JsonModel()
                    {
                        Message = StatusMessage.AddAppointment,
                        StatusCode = (int)HttpStatusCodes.OK,
                        data = new object()
                    };
                }
                else
                {
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            patientAppointment = _appointmentRepository.Get(x => x.Id == patientAppointmentModel.PatientAppointmentId && x.IsActive == true && x.IsDeleted == false);
                            if (!ReferenceEquals(patientAppointment, null))
                            {
                                action = "update";
                                patientAppointment.StartDateTime = patientAppointmentModel.StartDateTime;
                                patientAppointment.EndDateTime = patientAppointmentModel.EndDateTime;
                                //patientAppointment.AppointmentTypeID = patientAppointmentModel.AppointmentTypeID;
                                patientAppointment.AppointmentTypeID = null;
                                patientAppointment.Notes = patientAppointmentModel.Notes;
                                patientAppointment.PatientAddressID = patientAppointmentModel.PatientAddressID;
                                patientAppointment.ServiceLocationID = patientAppointmentModel.ServiceLocationID;
                                patientAppointment.OfficeAddressID = patientAppointmentModel.OfficeAddressID;
                                patientAppointment.CustomAddress = patientAppointmentModel.CustomAddress;
                                patientAppointment.CustomAddressID = patientAppointmentModel.CustomAddressID;
                                patientAppointment.Longitude = patientAppointmentModel.Longitude;
                                patientAppointment.Latitude = patientAppointmentModel.Latitude;
                                patientAppointment.ApartmentNumber = patientAppointmentModel.ApartmentNumber;
                                patientAppointment.IsTelehealthAppointment = true;
                                patientAppointment.IsDirectService = true;
                                patientAppointment.IsExcludedFromMileage = true;
                                //patientAppointment.IsDirectService = patientAppointmentModel.IsDirectService;
                                patientAppointment.UpdatedBy = tokenModel.UserID;
                                patientAppointment.UpdatedDate = DateTime.UtcNow;
                                //patientAppointment.IsTelehealthAppointment = patientAppointmentModel.IsTelehealthAppointment;
                                patientAppointment.Offset = patientAppointmentModel.OffSet;

                                //patientAppointment.IsExcludedFromMileage = patientAppointmentModel.IsExcludedFromMileage;
                                patientAppointment.DriveTime = patientAppointmentModel.DriveTime;
                                patientAppointment.Mileage = patientAppointmentModel.Mileage;
                                patientAppointment.PatientInsuranceId = patientAppointmentModel.PatientInsuranceId;
                                patientAppointment.AuthorizationId = patientAppointmentModel.AuthorizationId;
                                _appointmentRepository.Update(patientAppointment);
                                _appointmentRepository.SaveChanges();

                                if (isDelete)
                                    UpdateScheduledUnits(tokenModel, null, "delete", patientAppointment.Id);
                                if (list != null && list.Count > 0)
                                    UpdateScheduledUnits(tokenModel, list, "add", patientAppointment.Id);
                                appointmentStaffList = _appointmentStaffRepository.GetAll(x => x.PatientAppointmentID == patientAppointmentModel.PatientAppointmentId && x.IsActive == true && x.IsDeleted == false).ToList();
                                foreach (AppointmentStaffs aptStaff in patientAppointmentModel.AppointmentStaffs)
                                {
                                    appointmentStaffs = appointmentStaffList.Find(x => x.PatientAppointmentID == patientAppointmentModel.PatientAppointmentId && x.StaffID == aptStaff.StaffId);
                                    if (appointmentStaffs != null)
                                    {
                                        if (aptStaff.IsDeleted == true)
                                        {
                                            appointmentStaffs.IsDeleted = aptStaff.IsDeleted;
                                            appointmentStaffs.DeletedBy = tokenModel.UserID;
                                            appointmentStaffs.DeletedDate = DateTime.UtcNow;
                                        }
                                    }
                                    else
                                    {
                                        appointmentStaffs = new AppointmentStaff();
                                        appointmentStaffs.PatientAppointmentID = patientAppointmentModel.PatientAppointmentId;
                                        appointmentStaffs.StaffID = aptStaff.StaffId;
                                        appointmentStaffs.CreatedBy = tokenModel.UserID;
                                        appointmentStaffs.CreatedDate = DateTime.UtcNow;
                                        appointmentStaffNewList.Add(appointmentStaffs);
                                    }
                                }
                                if (appointmentStaffList.FindAll(x => x.IsDeleted == true).Count > 0)
                                    _appointmentStaffRepository.Update(appointmentStaffList.ToArray());
                                if (appointmentStaffNewList != null && appointmentStaffNewList.Count > 0)
                                    _appointmentStaffRepository.Create(appointmentStaffNewList.ToArray());

                                _appointmentStaffRepository.SaveChanges();
                                response = new JsonModel() { Message = StatusMessage.UpdateAppointment };


                                //Push notifications for mobile device

                                int appmnttstaffid = patientAppointmentModel.AppointmentStaffs.Select(x => x.StaffId).FirstOrDefault();
                                Staffs staffs = _staffRepository.Get(a => a.Id == appmnttstaffid && a.IsDeleted == false && a.IsActive == true);
                                var providerName = staffs.FirstName + " " + staffs.LastName;

                                var result = _context.SymptomatePatientReport.Where(s => s.PatientID == patientAppointmentModel.PatientAppointmentId).Select(s => s.Id).FirstOrDefault();
                                if (result != 0)
                                {
                                    IsSymptomateReportExist = true;
                                    ReportId = result;
                                }

                                int? userId = _context.Patients.Where(x => x.Id == patientAppointmentModel.PatientID && x.IsActive == true && x.IsDeleted == false).Select(x => x.UserID).FirstOrDefault();
                                string deviceToken = _context.User.Where(x => x.Id == userId).Select(x => x.DeviceToken).FirstOrDefault();
                                if (!string.IsNullOrWhiteSpace(deviceToken))
                                {
                                    PushMobileNotificationModel pushMobileNotification = new PushMobileNotificationModel();
                                    pushMobileNotification.DeviceToken = deviceToken;
                                    pushMobileNotification.Message = "Your Appointment has been rescheduled";
                                    pushMobileNotification.NotificationPriority = PushNotificationPriority.High;
                                    pushMobileNotification.NotificationType = CommonEnum.NotificationActionType.UpdateAppointment.ToString();
                                    PushNotificationsUserDetailsModel model = new PushNotificationsUserDetailsModel()
                                    {
                                        ProviderID = appmnttstaffid,
                                        PatientID = (int)patientAppointmentModel.PatientID,
                                        AppointmentId = patientAppointmentModel.PatientAppointmentId,
                                        ImageThumbnail = "",
                                        Name = providerName,
                                        Address ="",
                                        StartDate = patientAppointmentModel.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                                        EndDate = patientAppointmentModel.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                                        StatusName = "Rescheduled",
                                        IsSymptomateReportExist = IsSymptomateReportExist,
                                        ReportId = ReportId
                                    };
                                    pushMobileNotification.Data = model;
                                    PushNotificationsService.SendPushNotificationForMobile(pushMobileNotification);
                                }
                            }
                            else
                                response = new JsonModel() { Message = StatusMessage.AppointmentNotExists };

                            response.data = new object();
                            response.StatusCode = (int)HttpStatusCodes.OK;
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return response = new JsonModel()
                            {
                                data = new object(),
                                Message = StatusMessage.ServerError,
                                StatusCode = (int)HttpStatusCodes.InternalServerError
                            };
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                //This code has been added on 25April to avoid trnasaction conflicts in Context and DbCommand.
                //It should be removed once that issue will be resolved
                if (action != "" && action == "add")
                {
                    List<AppointmentAuthorization> authList = _appointmentAuthorizationRepository.GetAll(x => aptIds.Contains(x.AppointmentId)).ToList();
                    List<AppointmentStaff> aptStaff = _appointmentStaffRepository.GetAll(x => aptIds.Contains(x.PatientAppointmentID)).ToList();
                    List<PatientAppointment> aptList = _appointmentRepository.GetAll(x => aptIds.Contains(x.Id)).ToList();
                    if (aptStaff != null && aptStaff.Count > 0)
                        _appointmentStaffRepository.Delete(aptStaff.ToArray());
                    if (authList != null && authList.Count > 0)
                        _appointmentAuthorizationRepository.Delete(authList.ToArray());
                    if (aptList != null && aptList.Count > 0)
                        _appointmentRepository.Delete(aptList.ToArray());
                    _appointmentRepository.SaveChanges();
                }
                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.ServerError,
                    StatusCode = (int)HttpStatusCodes.InternalServerError,
                    AppError = ex.Message
                };
            }
            //}
        }





        private void UpdateScheduledUnits(TokenModel tokenModel, List<AppointmentAuthModel> list, string action, int appointmentId)
        {
            List<AppointmentAuthorization> authList = new List<AppointmentAuthorization>();
            AppointmentAuthorization appointmentAuthorization = null;
            if (action == "add")
            {
                list.ForEach(x =>
                {
                    appointmentAuthorization = new AppointmentAuthorization();
                    appointmentAuthorization.AppointmentId = appointmentId;
                    appointmentAuthorization.AuthProcedureCPTLinkId = Convert.ToInt32(x.AuthProcedureCPTLinkId);
                    appointmentAuthorization.ServiceCodeId = x.ServiceCodeId;
                    appointmentAuthorization.UnitsBlocked = x.UnitToBlock;
                    appointmentAuthorization.AuthScheduledDate = _appointmentRepository.Get(a => a.Id == appointmentId).StartDateTime;
                    appointmentAuthorization.CreatedBy = tokenModel.UserID;
                    appointmentAuthorization.CreatedDate = DateTime.UtcNow;
                    appointmentAuthorization.IsActive = true;
                    appointmentAuthorization.IsDeleted = false;
                    appointmentAuthorization.IsBlocked = true;
                    authList.Add(appointmentAuthorization);
                });
                _appointmentAuthorizationRepository.Create(authList.ToArray());
            }
            else if (action == "delete")
            {
                authList = _appointmentAuthorizationRepository.GetAll(x => x.AppointmentId == appointmentId && x.IsActive == true && x.IsDeleted == false).ToList();
                if (authList.Count > 0)
                    authList.ForEach(x => { x.IsDeleted = true; x.DeletedBy = tokenModel.UserID; x.DeletedDate = DateTime.UtcNow; });
                _appointmentAuthorizationRepository.Update(authList.ToArray());
            }
            else if (action == "update")
            {
                authList = _appointmentAuthorizationRepository.GetAll(x => x.AppointmentId == appointmentId && x.IsActive == true && x.IsDeleted == false).ToList();
                if (authList.Count > 0)
                    authList.ForEach(x => { x.IsBlocked = true; x.UpdatedBy = tokenModel.UserID; x.UpdatedDate = DateTime.UtcNow; });
                _appointmentAuthorizationRepository.Update(authList.ToArray());
            }
            else if (action == "cancel")
            {
                authList = _appointmentAuthorizationRepository.GetAll(x => x.AppointmentId == appointmentId && x.IsActive == true && x.IsDeleted == false).ToList();
                if (authList.Count > 0)
                    authList.ForEach(x => { x.IsBlocked = false; x.UpdatedBy = tokenModel.UserID; x.UpdatedDate = DateTime.UtcNow; });
                _appointmentAuthorizationRepository.Update(authList.ToArray());
            }
            if (authList.Count > 0)
                _appointmentAuthorizationRepository.SaveChanges();
        }

        public JsonModel DeleteAppointment(int appointmentId, int? parentAppointmentId, bool deleteSeries, bool isAdmin, TokenModel token)
        {
            try
            {
                List<AppointmentAuthModel> list = null;
                PatientAppointment patientAppointment = null;
                List<PatientAppointment> patientAppointmentList = null;
                if (appointmentId > 0)
                {
                    SQLResponseModel sqlResponse = _appointmentRepository.DeleteAppointment<SQLResponseModel>(appointmentId, isAdmin, deleteSeries, token).FirstOrDefault();
                    return new JsonModel()
                    {
                        data = new object(),
                        StatusCode = sqlResponse.StatusCode,
                        Message = sqlResponse.Message
                    };
                }

                if (!deleteSeries)
                {
                    patientAppointment = _appointmentRepository.Get(x => x.Id == appointmentId && x.IsActive == true && x.IsDeleted == false);
                    if (!ReferenceEquals(patientAppointment, null))
                    {
                        //if (patientAppointment.PatientID != null && patientAppointment.PatientID > 0)
                        //    //list = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)patientAppointment.PatientID, patientAppointment.AppointmentTypeID, patientAppointment.StartDateTime, patientAppointment.EndDateTime, InsurancePlanType.Primary.ToString()).Where(x => x.AuthProcedureCPTLinkId != null && x.AuthProcedureCPTLinkId > 0).ToList();
                        using (var transaction = _context.Database.BeginTransaction())
                        {
                            try
                            {
                                //if (list != null && list.Count > 0)
                                //if (!isAdmin)
                                //{
                                //patientAppointment.IsDeleted = true;
                                //patientAppointment.DeletedBy = token.UserID;
                                //patientAppointment.DeletedDate = DateTime.UtcNow;

                                //_appointmentRepository.Update(patientAppointment);
                                //_appointmentRepository.SaveChanges();
                                //}

                                UpdateScheduledUnits(token, list, "delete", appointmentId);  ///please change this for scheduled units
                                response = new JsonModel() { Message = StatusMessage.DeleteAppointment };
                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                                return response = new JsonModel()
                                {
                                    data = new object(),
                                    Message = StatusMessage.ServerError,
                                    StatusCode = (int)HttpStatusCodes.InternalServerError
                                };
                            }
                        }
                    }
                    else
                        response = new JsonModel() { Message = StatusMessage.AppointmentNotExists };
                }
                else
                {
                    if (parentAppointmentId != null)
                    {
                        //patientAppointmentList = _appointmentRepository.GetAll(x => x.ParentAppointmentID == parentAppointmentId && x.IsActive == true && x.IsDeleted == false && x.Id >= appointmentId).ToList();  //DateTime.ParseExact(x.StartDateTime.ToShortDateString(), "dd-MM-yyyy", null).CompareTo(DateTime.ParseExact(DateTime.UtcNow.ToShortDateString(), "dd-MM-yyyy", null))

                        PatientAppointment resPatientAppointment = _appointmentRepository.GetAll(x => x.Id == appointmentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        patientAppointmentList = _appointmentRepository.GetAll(x => x.ParentAppointmentID == parentAppointmentId && x.IsActive == true && x.IsDeleted == false && x.StartDateTime >= resPatientAppointment.StartDateTime).ToList();
                    }
                    //DateTime.Compare
                    else
                    {
                        patientAppointmentList = _appointmentRepository.GetAll(x => x.Id == appointmentId && x.IsActive == true && x.IsDeleted == false).ToList();
                        patientAppointmentList.AddRange(_appointmentRepository.GetAll(x => x.ParentAppointmentID == appointmentId && x.IsActive == true && x.IsDeleted == false).ToList());
                    }
                    if (patientAppointmentList != null && patientAppointmentList.Count > 0)
                    {
                        patientAppointmentList.ForEach(x => { x.IsDeleted = true; x.DeletedBy = token.UserID; x.DeletedDate = DateTime.UtcNow; });
                        //if (!isAdmin)
                        //{
                        //    _appointmentRepository.Update(patientAppointmentList.ToArray());
                        //    _appointmentRepository.SaveChanges();
                        //}
                        patientAppointmentList.ForEach(x =>
                        {
                            UpdateScheduledUnits(token, list, "delete", x.Id);  ///please change this for scheduled units
                        });
                        response = new JsonModel() { Message = StatusMessage.DeleteAppointmentRecurrence };
                    }
                    else
                        response = new JsonModel() { Message = StatusMessage.AppointmentNotExists };
                }
                response.data = new object();
                response.StatusCode = (int)HttpStatusCodes.OK;
                return response;
            }
            catch (Exception)
            {
                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.ServerError,
                    StatusCode = (int)HttpStatusCodes.InternalServerError
                };
            }
        }

        public JsonModel GetAppointmentDetails(int appointmentId, TokenModel token)
        {
            try
            {
                PatientAppointment pat = _appointmentRepository.GetByID(appointmentId);

                PatientAppointmentModel patientAppointmentModel = _appointmentRepository.GetAppointmentDetails<PatientAppointmentModel>(appointmentId).FirstOrDefault();

                LocationModel locationModal = _locationService.GetLocationOffsets(pat.ServiceLocationID, token);
                patientAppointmentModel.StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointmentModel.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token);
                patientAppointmentModel.EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointmentModel.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token);
                patientAppointmentModel.PatientPhotoThumbnailPath = CommonMethods.CreateImageUrl(token.Request, ImagesPath.PatientThumbPhotos, patientAppointmentModel.PatientPhotoThumbnailPath);
                patientAppointmentModel.AppointmentStaffs = !string.IsNullOrEmpty(patientAppointmentModel.XmlString) ? XDocument.Parse(patientAppointmentModel.XmlString).Descendants("Child").Select(y => new AppointmentStaffs()
                {
                    StaffId = Convert.ToInt32(y.Element("StaffId").Value),
                    StaffName = y.Element("StaffName").Value,
                    //PhotoThumbnail = CommonMethods.CreateImageUrl(token.Request, ImagesPath.StaffThumbPhotos, y.Element("PhotoThumbnailPath").Value)
                }).ToList() : new List<AppointmentStaffs>(); patientAppointmentModel.XmlString = null;
                return response = new JsonModel()
                {
                    data = patientAppointmentModel,
                    Message = StatusMessage.FetchMessage,
                    StatusCode = (int)HttpStatusCodes.OK
                };
            }
            catch (Exception ex)
            {
                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.ServerError,
                    StatusCode = (int)HttpStatusCodes.InternalServerError,
                    AppError = ex.Message
                };
            }
        }

        public JsonModel GetStaffAndPatientByLocation(string locationIds, string permissionKey, string isActiveCheckRequired, TokenModel token)
        {
            try
            {
                StaffPatientModel staffLocation = new StaffPatientModel();
                if (!string.IsNullOrEmpty(locationIds))
                    staffLocation = _patientAppointmentRepository.GetStaffAndPatientByLocation(locationIds, permissionKey, isActiveCheckRequired, token);

                return response = new JsonModel()
                {
                    data = staffLocation,
                    Message = StatusMessage.FetchMessage,
                    StatusCode = (int)HttpStatusCodes.OK
                };
            }
            catch (Exception)
            {

                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.ServerError,
                    StatusCode = (int)HttpStatusCodes.InternalServerError
                };
            }
        }
        public JsonModel GetStaffByLocation(string locationIds, string isActiveCheckRequired, TokenModel token)
        {
            List<StaffModel> staffslist = new List<StaffModel>();
            if (!string.IsNullOrEmpty(locationIds))
                staffslist = _patientAppointmentRepository.GetStaffByLocation<StaffModel>(locationIds, isActiveCheckRequired, token).ToList();

            return response = new JsonModel()
            {
                data = staffslist,
                Message = StatusMessage.FetchMessage,
                StatusCode = (int)HttpStatusCodes.OK
            };
        }


        public List<AvailabilityMessageModel> CheckIsValidAppointment(string staffIds, DateTime startDate, DateTime endDate, Nullable<DateTime> currentDate, Nullable<int> patientAppointmentId, Nullable<int> patientId, Nullable<int> appointmentTypeID, TokenModel token)
        {
            return _appointmentRepository.CheckIsValidAppointment<AvailabilityMessageModel>(staffIds, startDate, endDate, currentDate, patientAppointmentId, patientId, appointmentTypeID, token).ToList();
        }

        public List<AvailabilityMessageModel> CheckIsValidAppointmentWithLocation(string staffIds, DateTime startDate, DateTime endDate, Nullable<DateTime> currentDate, Nullable<int> patientAppointmentId, Nullable<int> patientId, Nullable<int> appointmentTypeID, decimal currentOffset, TokenModel token)
        {
            return _appointmentRepository.CheckIsValidAppointmentWithLocation<AvailabilityMessageModel>(staffIds, startDate, endDate, currentDate, patientAppointmentId, patientId, appointmentTypeID, currentOffset, token).ToList();
        }

        public JsonModel GetDataForSchedulerByPatient(Nullable<int> patientId, int locationId, DateTime startDate, DateTime endDate, Nullable<int> patientInsuranceId, TokenModel token)
        {
            Dictionary<string, object> schedulerData = new Dictionary<string, object>();
            try
            {
                LocationModel locationModal = _locationService.GetLocationOffsets(locationId, token);
                schedulerData.Add("PatientPayerActivities", _patientRepository.GetActivitiesForPatientPayer<AppointmentTypeModel>(patientId, InsurancePlanType.Primary.ToString(), CommonMethods.ConvertToUtcTimeWithOffset(startDate, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName), CommonMethods.ConvertToUtcTimeWithOffset(endDate, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName), patientInsuranceId, token).ToList());
                schedulerData.Add("PatientAddresses", _patientRepository.GetPatientAddressList<PatientAddressModel>(patientId, locationId).ToList());
                return response = new JsonModel()
                {
                    data = schedulerData,
                    Message = StatusMessage.FetchMessage,
                    StatusCode = (int)HttpStatusCodes.OK
                };
            }
            catch (Exception)
            {

                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.ServerError,
                    StatusCode = (int)HttpStatusCodes.InternalServerError
                };
            }
        }

        public JsonModel UpdateServiceCodeBlockedUnit(int authProcedureCPTLinkId, string action, TokenModel token)
        {
            try
            {
                int value = action.ToLower() == "add" ? 1 : -1;
                AuthProcedureCPT authProcedureCPT = _patientAuthorizationProcedureCPTLinkRepository.Get(x => x.Id == authProcedureCPTLinkId && x.IsActive == true && x.IsDeleted == false);
                if (!ReferenceEquals(authProcedureCPT, null))
                {
                    authProcedureCPT.BlockedUnit = (authProcedureCPT.BlockedUnit != null && authProcedureCPT.BlockedUnit > 0) ? authProcedureCPT.BlockedUnit + value : authProcedureCPT.BlockedUnit;
                    authProcedureCPT.UpdatedBy = token.UserID;
                    authProcedureCPT.UpdatedDate = DateTime.UtcNow;
                    _patientAuthorizationProcedureCPTLinkRepository.Update(authProcedureCPT);
                    _patientAuthorizationProcedureCPTLinkRepository.SaveChanges();
                }
                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.FetchMessage,
                    StatusCode = (int)HttpStatusCodes.OK
                };
            }
            catch
            {
                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.ServerError,
                    StatusCode = (int)HttpStatusCodes.InternalServerError
                };
            }
        }

        public JsonModel CancelAppointments(int[] appointmentIds, int CancelTypeId, string reson, TokenModel token)
        {
            try
            {
                int statusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, AppointmentStatus.CANCELLED, token);
                List<PatientAppointment> dbAppointments = _appointmentRepository.GetAll(a => appointmentIds.Contains(a.Id)).ToList();
                dbAppointments.ForEach(a => { a.CancelReason = reson; a.CancelTypeId = CancelTypeId; a.StatusId = statusId; a.UpdatedBy = token.UserID; a.UpdatedDate = DateTime.UtcNow; UpdateScheduledUnits(token, null, "cancel", a.Id); });
                _appointmentRepository.Update(dbAppointments.ToArray());
                var result = _appointmentRepository.SaveChanges();
                string error = string.Empty;
                if (result > 0)
                {
                    var org = (OrganizationDetailModel)_organizationService.GetOrganizationDetailsById(token).data;
                    for (var i = 0; i < appointmentIds.Length; i++)
                    {
                        var appPayment = _appointmentPaymentRepository.Get(p => p.AppointmentId == appointmentIds[i]);
                        if (appPayment != null)
                        {
                            StripeConfiguration.ApiKey = org.StripeSecretKey;
                            var options = new RefundCreateOptions
                            {
                                Charge = appPayment.PaymentToken,
                            };
                            var service = new RefundService();
                            var refund = service.Create(options);
                            if (!string.IsNullOrEmpty(refund.Id))
                            {
                                AppointmentPaymentRefund appointmentPaymentRefund = new AppointmentPaymentRefund()
                                {
                                    RefundToken = refund.Id,
                                    PaymentToken = appPayment.PaymentToken,
                                    AppointmentId = appointmentIds[i],
                                    CreatedBy = token.UserID,
                                    CreatedDate = DateTime.UtcNow,
                                    IsActive = true
                                };
                                _appointmentPaymentRefundRepository.Create(appointmentPaymentRefund);
                                var refundStatus = _appointmentPaymentRefundRepository.SaveChanges();
                            }

                            var patientAppointment = _appointmentRepository.GetByID((int)appointmentIds[i]);
                            LocationModel locationModal = _locationService.GetLocationOffsets(patientAppointment.ServiceLocationID, token);
                            PatientAppointmentModel patientAppointmentModel = new PatientAppointmentModel()
                            {
                                StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointment.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token),
                                EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointment.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token),
                                Mode = patientAppointment.BookingMode,
                                Type = patientAppointment.BookingType
                            };
                            var patientResponse = _patientService.GetPatientById((int)patientAppointment.PatientID, token);
                            PatientDemographicsModel patient;
                            if (patientResponse.StatusCode == (int)HttpStatusCodes.OK)
                                patient = (PatientDemographicsModel)patientResponse.data;
                            else
                                patient = null;
                            if (patient != null)
                            {
                                error = SendAppointmentEmail(patientAppointmentModel,
                                 patient.Email,
                                  patient.FirstName + " " + patient.LastName,
                                  (int)EmailType.BookAppointment,
                                  (int)EmailSubType.RejectApointment,
                                  patientAppointment.Id,
                                  "/templates/reject-appointment.html",
                                  "Appointment Cancelled",
                                  token,
                                  token.Request.Request
                                  );

                            }
                        }
                    }
                    var appointmentId = dbAppointments.Select(x => x.Id).FirstOrDefault();
                    int staffId = _appointmentStaffRepository.Get(a => a.PatientAppointmentID == appointmentId && a.IsDeleted == false && a.IsActive == true).StaffID;
                    string message = _notificationRepository.GetNotificationMessage(null, staffId, appointmentId, "rejected");
                    int patientId = (int)dbAppointments.Select(x => x.PatientID).FirstOrDefault();
                    Staffs staffs = _staffRepository.Get(a => a.Id == staffId && a.IsDeleted == false && a.IsActive == true);
                    var providerName = staffs.FirstName + " " + staffs.LastName;
                    PushNotificationModel saveNotificationModel = new PushNotificationModel()
                    {
                        //Message = NotificationMessage.AppointmentRejected + " " + providerName,
                        Message = message,
                        NotificationTypeId = (int)NotificationType.PushNotification,
                        TypeId = NotificationActionType.RejectAppointment,
                        SubTypeId = NotificationActionSubType.AppointmentRejectedByProvider,
                        PatientId = patientId,
                        NotificationType = NotificationType.PushNotification
                    };
                    _notificationService.SaveNotification(saveNotificationModel, token);

                    //PushNotifications for Mobile
                    int userId = (int)_context.Patients.Where(x => x.Id == patientId && x.IsDeleted == false && x.IsActive == true).Select(x => x.UserID).FirstOrDefault();
                    string deviceToken = _context.User.Where(x => x.Id == userId).Select(x => x.DeviceToken).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(deviceToken))
                    {
                        PushMobileNotificationModel pushMobileNotification = new PushMobileNotificationModel();
                        pushMobileNotification.DeviceToken = deviceToken;
                        pushMobileNotification.Message = message;
                        pushMobileNotification.NotificationPriority = PushNotificationPriority.High;
                        pushMobileNotification.NotificationType = CommonEnum.NotificationActionType.RejectAppointment.ToString();
                        PushNotificationsUserDetailsModel model = new PushNotificationsUserDetailsModel()
                        {
                            ProviderID = staffId,
                            PatientID = (int)dbAppointments.Select(x => x.PatientID).FirstOrDefault(),
                            AppointmentId = dbAppointments.Select(x => x.Id).FirstOrDefault(),
                            ImageThumbnail = string.IsNullOrEmpty(staffs.PhotoThumbnailPath) ? staffs.PhotoThumbnailPath : CommonMethods.CreateImageUrl(token.Request, ImagesPath.StaffThumbPhotos, staffs.PhotoThumbnailPath),
                            Name = providerName,
                            Address = staffs.Address,
                            StartDate = dbAppointments.Select(x => x.StartDateTime).FirstOrDefault().ToString("yyyy-MM-ddTHH:mm:ss"),
                            EndDate = dbAppointments.Select(x => x.EndDateTime).FirstOrDefault().ToString("yyyy-MM-ddTHH:mm:ss"),
                            StatusName = dbAppointments.Select(x => x.AppointmentStatus.GlobalCodeName).FirstOrDefault(),
                        };
                        pushMobileNotification.Data = model;
                        PushNotificationsService.SendPushNotificationForMobile(pushMobileNotification);
                    }
                    response = new JsonModel()
                    {
                        data = new object(),
                        Message = StatusMessage.CancelAppointment,
                        StatusCode = (int)HttpStatusCodes.OK
                    };
                    //if (!string.IsNullOrEmpty(error))
                    //    response.Message = error;
                    return response;
                }
                else
                {
                    return response = new JsonModel()
                    {
                        data = new object(),
                        Message = StatusMessage.ServerError,
                        StatusCode = (int)HttpStatusCodes.InternalServerError
                    };
                }

            }
            catch (Exception ex)
            {
                return response = new JsonModel()
                {
                    data = new object(),
                    Message = StatusMessage.ServerError,
                    StatusCode = (int)HttpStatusCodes.InternalServerError
                };
            }
        }

        public JsonModel ActivateAppointments(int appointmentId, bool isAdmin, TokenModel token)
        {
            PatientAppointment patientAppointment = _appointmentRepository.GetByID(appointmentId);
            List<AppointmentAuthModel> list = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)patientAppointment.PatientID, (int)patientAppointment.AppointmentTypeID, patientAppointment.StartDateTime, patientAppointment.EndDateTime, InsurancePlanType.Primary.ToString(), appointmentId, isAdmin, patientAppointment.PatientInsuranceId, patientAppointment.AuthorizationId).Where(x => x.AuthProcedureCPTLinkId != null && x.AuthProcedureCPTLinkId > 0).ToList();
            if (list != null && list.Count > 0 && list.First().AuthorizationMessage.ToLower() != "valid")
            {
                return new JsonModel()
                {
                    data = new object(),
                    Message = list.First().AuthorizationMessage,
                    StatusCode = (int)HttpStatusCodes.UnprocessedEntity
                };
            }
            PatientAppointment dbAppointments;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    dbAppointments = _appointmentRepository.Get(a => a.Id == appointmentId);
                    dbAppointments.CancelReason = string.Empty;
                    dbAppointments.CancelTypeId = null;
                    dbAppointments.UpdatedBy = token.UserID;
                    dbAppointments.UpdatedDate = DateTime.UtcNow;
                    _appointmentRepository.Update(dbAppointments);
                    _appointmentRepository.SaveChanges();

                    UpdateScheduledUnits(token, null, "update", appointmentId);  ///please change this for scheduled units

                    transaction.Commit();

                    response = new JsonModel()
                    {
                        data = new object(),
                        Message = StatusMessage.UndoCancelAppointment,
                        StatusCode = (int)HttpStatusCodes.OK
                    };

                }
                catch
                {
                    transaction.Rollback();
                    response = new JsonModel()
                    {
                        data = new object(),
                        Message = StatusMessage.ServerError,
                        StatusCode = (int)HttpStatusCodes.InternalServerError
                    };
                }
            }

            return response;
        }

        public JsonModel UpdateAppointmentStatus(AppointmentStatusModel appointmentStatusModel, TokenModel token)
        {
            response = new JsonModel(new object(), StatusMessage.ServerError, (int)HttpStatusCodes.InternalServerError, string.Empty);
            List<AppointmentAuthModel> list = null;
            PatientAppointment patientAppointment = null;
            patientAppointment = _appointmentRepository.Get(a => a.Id == appointmentStatusModel.Id);
            if (patientAppointment != null)
            {
                if (appointmentStatusModel.Status.ToLower() == AppointmentStatus.APPROVED.ToLower())
                {
                    int? appointmentId = patientAppointment.AppointmentTypeID == null ? 0 : patientAppointment.AppointmentTypeID;
                    list = _patientRepository.GetAuthDataForPatientAppointment<AppointmentAuthModel>((int)patientAppointment.PatientID, (int)appointmentId, patientAppointment.StartDateTime, patientAppointment.EndDateTime, InsurancePlanType.Primary.ToString(), patientAppointment.Id, false, null, null).Where(x => x.AuthProcedureCPTLinkId != null && x.AuthProcedureCPTLinkId > 0).ToList();
                    if (list != null && list.Count > 0 && list.First().AuthorizationMessage.ToLower() != "valid")
                    {
                        return new JsonModel(null, list.First().AuthorizationMessage, (int)HttpStatusCodes.UnprocessedEntity, string.Empty);
                    }
                }
                using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (appointmentStatusModel.Status.ToLower() == AppointmentStatus.INVITATION_REJECTED.ToLower() || appointmentStatusModel.Status.ToLower() == AppointmentStatus.INVITATION_ACCEPTED.ToLower())
                        {
                            patientAppointment.StatusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, appointmentStatusModel.Status, token, false);
                            patientAppointment.InvitationAcceptRejectRemarks = appointmentStatusModel.Notes;
                        }
                        else
                            patientAppointment.StatusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, appointmentStatusModel.Status, token);
                        patientAppointment.UpdatedBy = token.UserID;
                        patientAppointment.UpdatedDate = DateTime.UtcNow;
                        _appointmentRepository.Update(patientAppointment);
                        _appointmentRepository.SaveChanges();
                        if (appointmentStatusModel.Status.ToLower() == AppointmentStatus.APPROVED.ToLower())
                            UpdateScheduledUnits(token, list, "add", appointmentStatusModel.Id);
                        transaction.Commit();
                        response = new JsonModel(new object(), StatusMessage.UpdateAppointmentStatus, (int)HttpStatusCodes.OK);
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
                if (response.StatusCode == (int)HttpStatusCodes.OK)
                {
                    if (appointmentStatusModel.Status.ToLower() == AppointmentStatus.APPROVED.ToLower())
                    {
                        var patientResponse = _patientService.GetPatientById((int)patientAppointment.PatientID, token);
                        PatientDemographicsModel patient;
                        if (patientResponse.StatusCode == (int)HttpStatusCodes.OK)
                            patient = (PatientDemographicsModel)patientResponse.data;
                        else
                            patient = null;
                        LocationModel locationModal = _locationService.GetLocationOffsets(patientAppointment.ServiceLocationID, token);
                        PatientAppointmentModel patientAppointmentModel = new PatientAppointmentModel()
                        {
                            StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointment.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token),
                            EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointment.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token),
                            Mode = patientAppointment.BookingMode,
                            Type = patientAppointment.BookingType
                        };

                        if (patient != null)
                        {
                            //Chat Room
                            var addProviderInChatRoom = _chatService.GetChatRoomId(appointmentStatusModel.Id, token.UserID, token);
                            var addPatientInChatRoom = _chatService.GetChatRoomId(appointmentStatusModel.Id, patient.UserID, token);

                            //Create Provider Session For Video Call
                            var providerSession = _telehealthService.GetTelehealthSession(appointmentStatusModel.Id, token);

                            //Create Client Session For Video Call
                            var user = _userRepository.GetUserByID(patient.UserID);
                            TokenModel clientTokenModel = new TokenModel()
                            {
                                UserID = patient.UserID,
                                OrganizationID = token.OrganizationID,
                                RoleID = user.RoleID
                            };
                            var clientSession = _telehealthService.GetTelehealthSession(appointmentStatusModel.Id, clientTokenModel);

                            string error = SendAppointmentEmail(patientAppointmentModel,
                                                                 patient.Email,
                                                                  patient.FirstName + " " + patient.LastName,
                                                                  (int)EmailType.BookAppointment,
                                                                  (int)EmailSubType.AcceptApointment,
                                                                  appointmentStatusModel.Id,
                                                                  "/templates/accept-appointment.html",
                                                                  "Appointment Confirmed",
                                                                  token,
                                                                  token.Request.Request
                                                                  );
                        }
                        AppointmentStaff appointmentStaff = new AppointmentStaff();
                        int staffId = _appointmentStaffRepository.Get(a => a.PatientAppointmentID == appointmentStatusModel.Id && a.IsDeleted == false && a.IsActive == true).StaffID;

                        string message = _notificationRepository.GetNotificationMessage(null, staffId, appointmentStatusModel.Id, "approved");
                        Staffs staffs = _staffRepository.Get(a => a.Id == staffId && a.IsDeleted == false && a.IsActive == true);
                        patientAppointment = _appointmentRepository.Get(a => a.Id == appointmentStatusModel.Id);
                        var providerName = staffs.FirstName + " " + staffs.LastName;
                        PushNotificationModel saveNotificationModel = new PushNotificationModel()
                        {
                            //Message = NotificationMessage.AppointmentAccepted + " " + providerName,
                            Message = message,
                            NotificationTypeId = (int)NotificationType.PushNotification,
                            TypeId = NotificationActionType.AcceptAppointment,
                            SubTypeId = NotificationActionSubType.AppointmentAcceptedByProvider,
                            PatientId = patientAppointment.PatientID,
                            NotificationType = NotificationType.PushNotification
                        };
                        _notificationService.SaveNotification(saveNotificationModel, token);

                        //PushNotification for mobile
                        int userId = (int)_context.Patients.Where(x => x.Id == patientAppointment.PatientID && x.IsActive == true && x.IsDeleted == false).Select(x => x.UserID).FirstOrDefault();
                        string deviceToken = _context.User.Where(x => x.Id == userId).Select(x => x.DeviceToken).FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(deviceToken))
                        {
                            PushMobileNotificationModel pushMobileNotification = new PushMobileNotificationModel();
                            pushMobileNotification.DeviceToken = deviceToken;
                            pushMobileNotification.Message = message;
                            pushMobileNotification.NotificationPriority = PushNotificationPriority.High;
                            pushMobileNotification.NotificationType = CommonEnum.NotificationActionType.ApprovedAppointment.ToString();
                            PushNotificationsUserDetailsModel model = new PushNotificationsUserDetailsModel()
                            {
                                ProviderID = staffId,
                                PatientID = (int)patientAppointment.PatientID,
                                AppointmentId = patientAppointment.Id,
                                ImageThumbnail = string.IsNullOrEmpty(staffs.PhotoThumbnailPath) ? staffs.PhotoThumbnailPath : CommonMethods.CreateImageUrl(token.Request, ImagesPath.StaffThumbPhotos, staffs.PhotoThumbnailPath),
                                Name = providerName,
                                Address = staffs.Address,
                                StartDate = patientAppointment.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                                EndDate = patientAppointment.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                                StatusName = patientAppointment.AppointmentStatus.GlobalCodeName,
                                IsTelehealthAppointment = patientAppointment.IsTelehealthAppointment,
                                AppointmentTypeName = patientAppointment.BookingType
                            };
                            pushMobileNotification.Data = model;
                            PushNotificationsService.SendPushNotificationForMobile(pushMobileNotification);
                        }
                    }
                    else if (appointmentStatusModel.Status.ToLower() == AppointmentStatus.INVITATION_ACCEPTED.ToLower() || appointmentStatusModel.Status.ToLower() == AppointmentStatus.INVITATION_REJECTED.ToLower())
                    {
                        var staff = _staffRepository.GetStaffByUserId((int)patientAppointment.CreatedBy, token);
                        var staffLocations = _staffRepository.GetAssignedLocationsByStaffId(staff.Id, token);
                        int.TryParse(Common.CommonMethods.Decrypt(staffLocations.Where(x => x.IsDefault == true).FirstOrDefault().LocationId), out int locationId);
                        LocationModel locationModal = _locationService.GetLocationOffsets(locationId, token);
                        PatientAppointmentModel patientAppointmentModel = new PatientAppointmentModel()
                        {
                            StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointment.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token),
                            EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointment.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token),
                            Mode = patientAppointment.BookingMode,
                            Type = patientAppointment.BookingType
                        };

                        if (staff != null)
                        {
                            if (appointmentStatusModel.Status.ToLower() == AppointmentStatus.INVITATION_ACCEPTED.ToLower())
                            {

                                string error = SendAppointmentEmail(patientAppointmentModel,
                                                                     staff.Email,
                                                                      staff.FirstName + " " + staff.LastName,
                                                                      (int)EmailType.GroupSessionInvitation,
                                                                      (int)EmailSubType.AcceptInvitationApointment,
                                                                      appointmentStatusModel.Id,
                                                                      "/templates/accept-invitation-appointment.html",
                                                                      "Appointment Invitation Confirmed",
                                                                      token,
                                                                      token.Request.Request
                                                                      );
                                PushNotificationModel saveNotificationModel = new PushNotificationModel()
                                {
                                    Message = NotificationMessage.GroupSessionInvitationAccepted,
                                    NotificationTypeId = (int)NotificationType.TextNotification,
                                    TypeId = NotificationActionType.GroupSession,
                                    SubTypeId = NotificationActionSubType.GroupSessionInvitaionAccepted,
                                    StaffId = staff.Id,
                                    NotificationType = NotificationType.TextNotification
                                };
                                _notificationService.SaveNotification(saveNotificationModel, token);

                            }
                            if (appointmentStatusModel.Status.ToLower() == AppointmentStatus.INVITATION_REJECTED.ToLower())
                            {

                                string error = SendAppointmentEmail(patientAppointmentModel,
                                                                     staff.Email,
                                                                      staff.FirstName + " " + staff.LastName,
                                                                      (int)EmailType.GroupSessionInvitation,
                                                                      (int)EmailSubType.RejectInvitationApointment,
                                                                      appointmentStatusModel.Id,
                                                                      "/templates/reject-invitation-appointment.html",
                                                                      "Appointment Invitation Rejected",
                                                                      token,
                                                                      token.Request.Request
                                                                      );
                                PushNotificationModel saveNotificationModel = new PushNotificationModel()
                                {
                                    Message = NotificationMessage.GroupSessionInvitationRejected,
                                    NotificationTypeId = (int)NotificationType.TextNotification,
                                    TypeId = NotificationActionType.GroupSession,
                                    SubTypeId = NotificationActionSubType.GroupSessionInvitaionRejected,
                                    StaffId = staff.Id,
                                    NotificationType = NotificationType.TextNotification
                                };
                                _notificationService.SaveNotification(saveNotificationModel, token); 
                            }


                        }
                    }
                }
            }
            return response;
        }
       
        public JsonModel BookNewAppointementFromPatientPortal(PatientAppointmentModel patientAppointmentModel, TokenModel token)
        {
            JsonModel response = SaveAppointmentFromPatientPortal(patientAppointmentModel, token);
            if (response.StatusCode == (int)HttpStatusCodes.OK)
            {
                int.TryParse(response.data.ToString(), out int appointmentId);

                if (appointmentId <= 0)
                    return response;

                if(patientAppointmentModel.CouponCode!=null)
                {
                  
                    _assiggnedCouponsClientsRepository.UpdateNoOfCount<NoOfAttemptCouponCode>(patientAppointmentModel,token);
                }


                //AppointmentPaymentModel appointmentPaymentModel = new AppointmentPaymentModel()
                //{
                //    Amount = patientAppointmentModel.PayRate,
                //    AppointmentId = appointmentId,
                //    PaymentId = 0,
                //    PaymentMode = patientAppointmentModel.PaymentMode,
                //    PaymentToken = patientAppointmentModel.PaymentToken
                //};

                //var paymentResponse = _appointmentPaymentService.SaveUpdateAppointmentPayment(appointmentPaymentModel, token);
                //if (paymentResponse.StatusCode != (int)HttpStatusCodes.OK)
                //return paymentResponse;

                var staffResponse = _staffService.GetStaffProfileData(patientAppointmentModel.AppointmentStaffs[0].StaffId, token);
                StaffProfileModel staff;

                if (staffResponse.StatusCode == (int)HttpStatusCodes.OK)
                    staff = (StaffProfileModel)staffResponse.data;
                else
                    staff = null;

                var patientResponse = _patientService.GetPatientById(token.StaffID, token);
                PatientDemographicsModel patient;
                if (patientResponse.StatusCode == (int)HttpStatusCodes.OK)
                    patient = (PatientDemographicsModel)patientResponse.data;
                else
                    patient = null;

                if (staff != null)
                {
                    string error = SendAppointmentEmail(patientAppointmentModel,
                        staff.Email,
                         staff.FirstName + " " + staff.LastName,
                         (int)EmailType.BookAppointment,
                         (int)EmailSubType.BookAppointmentToProvider,
                         appointmentId,
                         "/templates/book-appointment-provider.html",
                         "New Appoointment Booked",
                         token,
                         token.Request.Request
                         );
                    //if (!string.IsNullOrEmpty(error)) response.Message = error;
                }
                if (patient != null)
                {

                    string error = SendAppointmentEmail(patientAppointmentModel,
                        patient.Email,
                         patient.FirstName + " " + patient.LastName,
                         (int)EmailType.BookAppointment,
                         (int)EmailSubType.BookAppointmentToClient,
                         appointmentId,
                         "/templates/book-appointment-client.html",
                         "Appoointment Booked Successfully",
                         token,
                         token.Request.Request
                         );
                    //if (!string.IsNullOrEmpty(error)) response.Message = error;
                }

                int staffId = patientAppointmentModel.AppointmentStaffs.Select(x => x.StaffId).FirstOrDefault();
                PatientAppointment appointmentdetails = new PatientAppointment();
                appointmentdetails = _context.PatientAppointment.Where(x => x.Id == appointmentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                string fullName = patient.FirstName + " " + patient.LastName;
                string message = _notificationRepository.GetNotificationMessage(fullName, null, appointmentId, "requested");
                PushNotificationModel saveNotificationModel = new PushNotificationModel()
                {
                    Message = message,
                    NotificationTypeId = (int)NotificationType.PushNotification,
                    TypeId = NotificationActionType.RequestAppointment,
                    SubTypeId = NotificationActionSubType.RequestAppointment,
                    StaffId = staffId,
                    NotificationType = NotificationType.PushNotification
                };
                _notificationService.SaveNotification(saveNotificationModel, token);



                if (patientAppointmentModel.IsSymptomaticProcess==false)
                {
                    //Push notifications for mobile device
                    int userId = _context.Staffs.Where(x => x.Id == staffId && x.IsActive == true && x.IsDeleted == false).Select(x => x.UserID).FirstOrDefault();
                    string deviceToken = _context.User.Where(x => x.Id == userId).Select(x => x.DeviceToken).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(deviceToken))
                    {
                        PushMobileNotificationModel pushMobileNotification = new PushMobileNotificationModel();
                        pushMobileNotification.DeviceToken = deviceToken;
                        pushMobileNotification.Message = message;
                        pushMobileNotification.NotificationPriority = PushNotificationPriority.High;
                        pushMobileNotification.NotificationType = CommonEnum.NotificationActionType.RequestAppointment.ToString();
                        PushNotificationsUserDetailsModel model = new PushNotificationsUserDetailsModel()
                        {
                            ProviderID = staffId,
                            PatientID = (int)appointmentdetails.PatientID,
                            AppointmentId = appointmentdetails.Id,
                            ImageThumbnail = string.IsNullOrEmpty(patient.PhotoThumbnailPath) ? patient.PhotoThumbnailPath : CommonMethods.CreateImageUrl(token.Request, ImagesPath.PatientThumbPhotos, patient.PhotoThumbnailPath),
                            Name = fullName,
                            Address = patientAppointmentModel.Address,
                            StartDate = appointmentdetails.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                            EndDate = appointmentdetails.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                            StatusName = appointmentdetails.AppointmentStatus.GlobalCodeName,
                            IsTelehealthAppointment= appointmentdetails.IsTelehealthAppointment,
                            AppointmentTypeName= appointmentdetails.BookingType
                        };
                        pushMobileNotification.Data = model;
                        PushNotificationsService.SendPushNotificationForMobile(pushMobileNotification);
                    }
                }
                
            }
            return response;
        }
        private string SendAppointmentEmail(PatientAppointmentModel patientAppointmentModel, string toEmail, string username, int emailType, int emailSubType, int primaryId, string templatePath, string subject, TokenModel tokenModel, HttpRequest Request)
        {
            //Get Current Login User Organization
            tokenModel.Request.Request.Headers.TryGetValue("BusinessToken", out StringValues businessName);
            Organization organization = _tokenService.GetOrganizationByOrgId(tokenModel.OrganizationID, tokenModel);

            //Get Current User Smtp Details
            OrganizationSMTPDetails organizationSMTPDetail = _organizationSMTPRepository.Get(a => a.OrganizationID == tokenModel.OrganizationID && a.IsDeleted == false && a.IsActive == true);
            OrganizationSMTPCommonModel organizationSMTPDetailModel = new OrganizationSMTPCommonModel();
            AutoMapper.Mapper.Map(organizationSMTPDetail, organizationSMTPDetailModel);
            organizationSMTPDetailModel.SMTPPassword = CommonMethods.Decrypt(organizationSMTPDetailModel.SMTPPassword);

            var osNameAndVersion = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            var emailHtml = System.IO.File.ReadAllText(_env.WebRootPath + templatePath);
            //#if DEBUG

            //#else
            //            emailHtml = emailHtml.Replace("{{action_url}}",tokenModel.DomainName+ "/register" + "?token=" + invitaionId);
            //#endif
            var hostingServer = _configuration.GetSection("HostingServer").Value;
            emailHtml = emailHtml.Replace("{{img_url}}", hostingServer + "img/cbimage.jpg");
            emailHtml = emailHtml.Replace("{{username}}", username);
            emailHtml = emailHtml.Replace("{{operating_system}}", osNameAndVersion);
            emailHtml = emailHtml.Replace("{{browser_name}}", Request.Headers["User-Agent"].ToString());
            emailHtml = emailHtml.Replace("{{organizationName}}", organization.OrganizationName);
            emailHtml = emailHtml.Replace("{{organizationEmail}}", organization.Email);
            emailHtml = emailHtml.Replace("{{organizationPhone}}", organization.ContactPersonPhoneNumber);
            emailHtml = emailHtml.Replace("{{AppDate}}", patientAppointmentModel.StartDateTime.ToString("MMM dd, yyyy"));
            emailHtml = emailHtml.Replace("{{AppTime}}", patientAppointmentModel.StartDateTime.ToShortTimeString() + " - " + patientAppointmentModel.EndDateTime.ToShortTimeString());
            emailHtml = emailHtml.Replace("{{AppMode}}", patientAppointmentModel.Mode);
            emailHtml = emailHtml.Replace("{{AppType}}", patientAppointmentModel.Type);
            EmailModel emailModel = new EmailModel
            {
                EmailBody = CommonMethods.Encrypt(emailHtml),
                ToEmail = CommonMethods.Encrypt(toEmail),
                EmailSubject = CommonMethods.Encrypt(subject),
                EmailType = emailType,
                EmailSubType = emailSubType,
                PrimaryId = primaryId,
                CreatedBy = tokenModel.UserID
            };
            ////Send Email
            ////await _emailSender.SendEmailAsync(userInvitationModel.Email, string.Format("Invitation From {0}", orgData.OrganizationName), emailHtml, organizationSMTPDetailModel, orgData.OrganizationName);
            //var isEmailSent = _emailSender.SendEmails(toEmail, subject, emailHtml, organizationSMTPDetailModel, organization.OrganizationName).Result; //_emailSender.SendEmail(organizationSMTPDetailModel.SMTPUserName, subject, emailHtml, organizationSMTPDetailModel, organization.OrganizationName, toEmail);
            //emailModel.EmailStatus = isEmailSent;
            ////Maintain Email log into Db
            ////var email = _emailWriteService.SaveEmailLog(emailModel, tokenModel);
            //return isEmailSent;
            var error = _emailSender.SendEmails(toEmail, subject, emailHtml, organizationSMTPDetailModel, organization.OrganizationName).Result; //_emailSender.SendEmail(organizationSMTPDetailModel.SMTPUserName, subject, emailHtml, organizationSMTPDetailModel, organization.OrganizationName, toEmail);
            if (!string.IsNullOrEmpty(error))
                emailModel.EmailStatus = false;
            else
                emailModel.EmailStatus = true;
            //Maintain Email log into Db
            var email = _emailWriteService.SaveEmailLog(emailModel, tokenModel);
            return error;
        }
        public JsonModel SaveAppointmentFromPatientPortal(PatientAppointmentModel patientAppointmentModel, TokenModel token)
        {
           
            var organizationJsonModel = _organizationService.GetOrganizationDetailsById(token);
            LocationModel locationModal = _locationService.GetLocationOffsets(patientAppointmentModel.ServiceLocationID, token);
            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    PatientAppointment patientAppointment = null;
                    AppointmentStaff appointmentStaff = null;
                    // GetLocationOffsets(patientAppointmentModel.ServiceLocationID);
                    if (patientAppointmentModel.PatientAppointmentId == 0)
                    {
                        patientAppointment = new PatientAppointment();
                        AutoMapper.Mapper.Map(patientAppointmentModel, patientAppointment);
                        if (patientAppointment.AppointmentTypeID == 0)
                            patientAppointment.AppointmentTypeID = null;
                        patientAppointment.StartDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointment.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                        patientAppointment.EndDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointment.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                        patientAppointment.OrganizationID = token.OrganizationID;
                        patientAppointment.IsActive = true;
                        patientAppointment.CreatedBy = token.UserID;
                        patientAppointment.CreatedDate = DateTime.UtcNow;
                        patientAppointment.Offset = (int)CommonMethods.GetCurrentOffset(patientAppointment.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                        patientAppointment.StatusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, AppointmentStatus.PENDING, token);
                        patientAppointment.IsClientRequired = true;
                        patientAppointment.BookingMode = patientAppointmentModel.Mode;
                        patientAppointment.BookingType = patientAppointmentModel.Type;
                        _appointmentRepository.Create(patientAppointment);
                        _appointmentRepository.SaveChanges();

                        appointmentStaff = patientAppointmentModel.AppointmentStaffs.Select(x => new AppointmentStaff() { StaffID = x.StaffId, PatientAppointmentID = patientAppointment.Id, CreatedBy = token.UserID, CreatedDate = DateTime.UtcNow, IsActive = true }).FirstOrDefault();
                        _appointmentStaffRepository.Create(appointmentStaff);
                        _appointmentStaffRepository.SaveChanges();

                        response = new JsonModel(patientAppointment.Id, StatusMessage.AddPatientAppointment, (int)HttpStatusCodes.OK, string.Empty);
                    }
                    else
                    {
                        patientAppointment = _appointmentRepository.Get(x => x.Id == patientAppointmentModel.PatientAppointmentId && x.IsActive == true && x.IsDeleted == false);
                        if (!ReferenceEquals(patientAppointment, null))
                        {
                            patientAppointment.StartDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointmentModel.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                            patientAppointment.EndDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointmentModel.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                            ///patientAppointment.AppointmentTypeID = patientAppointmentModel.AppointmentTypeID;
                            patientAppointment.AppointmentTypeID = null;
                            patientAppointment.Notes = patientAppointmentModel.Notes;
                            patientAppointment.PatientAddressID = patientAppointmentModel.PatientAddressID;
                            patientAppointment.ServiceLocationID = patientAppointmentModel.ServiceLocationID;
                            patientAppointment.OfficeAddressID = patientAppointmentModel.OfficeAddressID;
                            patientAppointment.CustomAddress = patientAppointmentModel.CustomAddress;
                            patientAppointment.CustomAddressID = patientAppointmentModel.CustomAddressID;
                            patientAppointment.Longitude = patientAppointmentModel.Longitude;
                            patientAppointment.Latitude = patientAppointmentModel.Latitude;
                            patientAppointment.ApartmentNumber = patientAppointmentModel.ApartmentNumber;
                            //patientAppointment.IsDirectService = patientAppointmentModel.IsDirectService;
                            patientAppointment.IsTelehealthAppointment = true;
                            patientAppointment.IsDirectService = true;
                            patientAppointment.IsExcludedFromMileage = true;
                            patientAppointment.IsClientRequired = true;
                            patientAppointment.UpdatedBy = token.UserID;
                            patientAppointment.UpdatedDate = DateTime.UtcNow;
                            patientAppointment.Offset = (int)CommonMethods.GetCurrentOffset(patientAppointmentModel.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                            _appointmentRepository.Update(patientAppointment);
                            _appointmentRepository.SaveChanges();

                            appointmentStaff = _appointmentStaffRepository.GetAll(x => x.PatientAppointmentID == patientAppointmentModel.PatientAppointmentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (appointmentStaff != null)
                            {
                                appointmentStaff.StaffID = patientAppointmentModel.AppointmentStaffs.FirstOrDefault().StaffId;
                                appointmentStaff.UpdatedBy = token.UserID;
                                appointmentStaff.UpdatedDate = DateTime.UtcNow;
                                _appointmentStaffRepository.Update(appointmentStaff);
                                _appointmentStaffRepository.SaveChanges();
                            }
                            response = new JsonModel() { Message = StatusMessage.UpdatePatientAppointment };
                        }

                        response.data = null;
                        response.StatusCode = (int)HttpStatusCodes.OK;

                        
                    }
                    if (response.StatusCode == (int)HttpStatusCodes.OK && !string.IsNullOrEmpty(patientAppointmentModel.PaymentToken))
                    {
                        #region Payment Save
                        if (organizationJsonModel.StatusCode != (int)HttpStatusCode.OK)
                            throw new Exception(StatusMessage.NotFound);

                        var organization = (OrganizationDetailModel)organizationJsonModel.data;
                        if (organization == null)
                            throw new Exception(StatusMessage.NotFound);
                        StripeConfiguration.ApiKey = organization.StripeSecretKey;
                        var tokenService = new TokenService();
                        Stripe.Token stripeToken = tokenService.Get(patientAppointmentModel.PaymentToken);

                        var options = new ChargeCreateOptions
                        {
                            Amount = (long?)patientAppointmentModel.PayRate * 100,
                            Currency = "inr",
                            Source = stripeToken.Id,
                            Description = "Appointment Fee",
                            ReceiptEmail = stripeToken.Card.Name,
                            Shipping = new ChargeShippingOptions
                            {
                                Address = new AddressOptions
                                {
                                    Country = stripeToken.Card.Country,
                                    City = "Chandigarh",
                                    Line1 = "House No 42",
                                    Line2 = "Mohali",
                                    PostalCode = "0123065",
                                    State = "Punjab"
                                },
                                Name = stripeToken.Card.Name
                            }

                        };
                        var service = new ChargeService();
                        var charge = service.Create(options);

                        AppointmentPaymentModel appointmentPaymentModel = new AppointmentPaymentModel()
                        {
                            Amount = patientAppointmentModel.PayRate,
                            AppointmentId = patientAppointment.Id,
                            PaymentId = 0,
                            PaymentMode = patientAppointmentModel.PaymentMode,
                            PaymentToken = charge.Id
                        };

                        if (appointmentPaymentModel.PaymentId == 0)
                        {
                            var tokenPayment = _appointmentPaymentRepository.GetAppointmentPaymentsByPaymentToken(appointmentPaymentModel.PaymentToken, token);
                            if (tokenPayment != null)
                                throw new Exception(StatusMessage.AppointmentPaymentTokenExisted);
                        }
                        var appointmentPayment = _mapper.Map<AppointmentPayments>(appointmentPaymentModel);
                        appointmentPayment.OrganizationId = token.OrganizationID;
                        appointmentPayment.CommissionPercentage = organization.BookingCommision;
                        appointmentPayment.CreatedBy = token.UserID;
                        appointmentPayment.CreatedDate = DateTime.UtcNow;
                        _appointmentPaymentRepository.Create(appointmentPayment);
                        _appointmentPaymentRepository.SaveChanges();


                        #endregion Payment Save
                    }
                    transaction.Commit();

                   
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    response = new JsonModel(null, StatusMessage.ServerError, (int)HttpStatusCodes.InternalServerError, ex.Message);
                }
            }
            return response;
        }

        public JsonModel DeleteAppointment(int appointmentId, TokenModel token)
        {
            PatientAppointment patientAppointment = _appointmentRepository.Get(x => x.Id == appointmentId && x.IsActive == true && x.IsDeleted == false);
            if (!ReferenceEquals(patientAppointment, null))
            {
                patientAppointment.IsDeleted = true;
                patientAppointment.DeletedBy = token.UserID;
                patientAppointment.DeletedDate = DateTime.UtcNow;
                _appointmentRepository.Update(patientAppointment);
                _appointmentRepository.SaveChanges();
                response = new JsonModel(null, StatusMessage.DeletePatientAppointment, (int)HttpStatusCodes.OK, string.Empty);
            }
            return response;
        }

        public LocationModel GetLocationOffsets(int? locationId)
        {
            LocationModel locationModal = new LocationModel();
            Location location = _locationRepository.GetByID(locationId);
            if (location != null)
            {
                locationModal.DaylightOffset = (((decimal)location.DaylightSavingTime) * 60);
                locationModal.StandardOffset = (((decimal)location.StandardTime) * 60);
            }

            return locationModal;
        }

        public JsonModel GetPendingAppointmentList(PatientAppointmentFilterModel appointmentFilterModel, TokenModel token)
        {
            List<PendingAppointmentViewModel> list = new List<PendingAppointmentViewModel>();

            list = _appointmentRepository.GetPendingAppointmentList<PendingAppointmentViewModel>(appointmentFilterModel, token).ToList();
            if (list != null && list.Count() > 0)
            {
                foreach (var x in list)
                {
                    LocationModel locationModal = _locationService.GetLocationOffsets(x.ServiceLocationID, token);
                    x.StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(x.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token);
                    x.EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(x.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName, token);

                    //x.StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(x.StartDateTime, CommonMethods.ConvertOffsetToMinutes(x.DaylightSavingTime), CommonMethods.ConvertOffsetToMinutes(x.StandardTime), x.TimeZoneName, token);
                    //x.EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(x.EndDateTime, CommonMethods.ConvertOffsetToMinutes(x.DaylightSavingTime), CommonMethods.ConvertOffsetToMinutes(x.StandardTime), x.TimeZoneName, token);

                    x.Address = x.Address == null ? "" : x.Address;
                    var result = _context.SymptomatePatientReport.Where(s => s.PatientID == x.PatientAppointmentId).Select(s => s.Id).FirstOrDefault();
                    if (result != 0)
                    {
                        x.IsSymptomateReportExist = true;
                        x.ReportId = result;
                    }

                    x.PendingAppointmentStaffs = !string.IsNullOrEmpty(x.XmlString) ? XDocument.Parse(x.XmlString).Descendants("Child").Select(y => new PendingAppointmentStaffs()
                    {
                        StaffId = Convert.ToInt32(y.Element("StaffId").Value),
                        StaffName = y.Element("StaffName").Value,
                        Address = y.Element("Address").Value,
                        ProviderImage = y.Element("PhotoPath") == null ? "" : CommonMethods.CreateImageUrl(token.Request, ImagesPath.StaffPhotos, y.Element("PhotoPath").Value),
                        ProviderImageThumbnail = y.Element("PhotoThumbnailPath") == null ? "" : CommonMethods.CreateImageUrl(token.Request, ImagesPath.StaffPhotos, y.Element("PhotoThumbnailPath").Value),
                    }).ToList() : new List<PendingAppointmentStaffs>(); x.XmlString = null;
                }

                //list.ForEach(x =>
                //{
                //    x.StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(x.StartDateTime, CommonMethods.ConvertOffsetToMinutes(x.DaylightSavingTime), CommonMethods.ConvertOffsetToMinutes(x.StandardTime), x.TimeZoneName, token);
                //    x.EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(x.EndDateTime, CommonMethods.ConvertOffsetToMinutes(x.DaylightSavingTime), CommonMethods.ConvertOffsetToMinutes(x.StandardTime), x.TimeZoneName, token);
                //    x.Address = x.Address == null ? "" : x.Address;
                //    x.PendingAppointmentStaffs = !string.IsNullOrEmpty(x.XmlString) ? XDocument.Parse(x.XmlString).Descendants("Child").Select(y => new PendingAppointmentStaffs()
                //    {
                //        StaffId = Convert.ToInt32(y.Element("StaffId").Value),
                //        StaffName = y.Element("StaffName").Value,
                //        Address = y.Element("Address").Value,
                //        ProviderImage = y.Element("PhotoPath") == null ? "" : CommonMethods.CreateImageUrl(token.Request, ImagesPath.StaffPhotos, y.Element("PhotoPath").Value),
                //        ProviderImageThumbnail = y.Element("PhotoThumbnailPath") == null ? "" : CommonMethods.CreateImageUrl(token.Request, ImagesPath.StaffPhotos, y.Element("PhotoThumbnailPath").Value),
                //    }).ToList() : new List<PendingAppointmentStaffs>(); x.XmlString = null;
                //});
            }
            response = new JsonModel(list, StatusMessage.FetchMessage, (int)HttpStatusCodes.OK);
            response.meta = new Meta(list, appointmentFilterModel);
            return response;
        }
        public JsonModel GetPastAndUpcomingAppointmentsList(int patientId, DateTime dateTime, TokenModel token)
        {
            List<PastAndUpcomingAppointmentModel> list = new List<PastAndUpcomingAppointmentModel>();

            list = _appointmentRepository.GetPastAndUpcomingAppointmentsList<PastAndUpcomingAppointmentModel>(patientId, dateTime, token).ToList();
            if (list != null && list.Count() > 0)
            {
                list.ForEach(x =>
                {
                    x.StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(x.StartDateTime, CommonMethods.ConvertOffsetToMinutes(x.DaylightSavingTime), CommonMethods.ConvertOffsetToMinutes(x.StandardTime), x.TimeZoneName, token);
                    x.EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(x.EndDateTime, CommonMethods.ConvertOffsetToMinutes(x.DaylightSavingTime), CommonMethods.ConvertOffsetToMinutes(x.StandardTime), x.TimeZoneName, token);

                    if (!string.IsNullOrEmpty(x.StaffImageUrl))
                        x.StaffImageUrl = CommonMethods.CreateImageUrl(token.Request, ImagesPath.StaffThumbPhotos, x.StaffImageUrl);
                    else x.StaffImageUrl = string.Empty;
                });
            }
            return new JsonModel(list, StatusMessage.FetchMessage, (int)HttpStatusCodes.OK);
        }
        public JsonModel SaveAppointmentWhenStaffInvitedForGroupSession(int staffId, int appointmentId, TokenModel token)
        {
            PatientAppointmentModel patientAppointmentModel = _appointmentRepository.GetAppointmentDetails<PatientAppointmentModel>(appointmentId).FirstOrDefault();
            var staffLocations = _staffRepository.GetAssignedLocationsByStaffId(staffId, token);

            LocationModel loggedInlocation = _locationService.GetLocationOffsets(patientAppointmentModel.ServiceLocationID, token);
            patientAppointmentModel.StartDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointmentModel.StartDateTime, loggedInlocation.DaylightOffset, loggedInlocation.StandardOffset, loggedInlocation.TimeZoneName, token);
            patientAppointmentModel.EndDateTime = CommonMethods.ConvertFromUtcTimeWithOffset(patientAppointmentModel.EndDateTime, loggedInlocation.DaylightOffset, loggedInlocation.StandardOffset, loggedInlocation.TimeZoneName, token);

            int.TryParse(Common.CommonMethods.Decrypt(staffLocations.Where(x => x.IsDefault == true).FirstOrDefault().LocationId), out int locationId);
            patientAppointmentModel.ServiceLocationID = locationId;
            LocationModel locationModal = _locationService.GetLocationOffsets(locationId, token);

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    patientAppointmentModel.PatientAppointmentId = 0;
                    PatientAppointment patientAppointment = null;
                    AppointmentStaff appointmentStaff;
                    patientAppointment = new PatientAppointment();
                    Mapper.Map(patientAppointmentModel, patientAppointment);
                    if (patientAppointment.AppointmentTypeID == 0)
                        patientAppointment.AppointmentTypeID = null;
                    patientAppointment.StartDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointment.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                    patientAppointment.EndDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointment.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                    patientAppointment.OrganizationID = token.OrganizationID;
                    patientAppointment.IsActive = true;
                    patientAppointment.CreatedBy = token.UserID;
                    patientAppointment.CreatedDate = DateTime.UtcNow;
                    patientAppointment.Offset = (int)CommonMethods.GetCurrentOffset(patientAppointment.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                    patientAppointment.StatusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, AppointmentStatus.INVITED, token, false);
                    patientAppointment.IsClientRequired = true;
                    patientAppointment.BookingMode = patientAppointmentModel.Mode;
                    patientAppointment.BookingType = patientAppointmentModel.Type;
                    patientAppointment.InvitationAppointentId = appointmentId;
                    _appointmentRepository.Create(patientAppointment);
                    _appointmentRepository.SaveChanges();
                    appointmentStaff = new AppointmentStaff() { StaffID = staffId, PatientAppointmentID = patientAppointment.Id, CreatedBy = token.UserID, CreatedDate = DateTime.UtcNow, IsActive = true };
                    _appointmentStaffRepository.Create(appointmentStaff);
                    _appointmentStaffRepository.SaveChanges();

                    response = new JsonModel(patientAppointment.Id, StatusMessage.AddPatientAppointment, (int)HttpStatusCodes.OK, string.Empty);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    response = new JsonModel(null, StatusMessage.ServerError, (int)HttpStatusCodes.InternalServerError, ex.Message);
                }
            }
            return response;
        }


        public JsonModel UpdateAppointmentFromPatientPortal(PatientAppointmentModel patientAppointmentModel, TokenModel token)
        {
            bool IsSymptomateReportExist = false;
            int ReportId = 0;
            JsonModel response = SaveAppointmentFromPatientPortal(patientAppointmentModel, token);
            if (response.StatusCode == (int)HttpStatusCodes.OK)
            {
                //int.TryParse(response.data.ToString(), out int appointmentId);

                //if (appointmentId <= 0)
                //    return response;


                //Push notifications for mobile device
                int? apptmntpatientid = patientAppointmentModel.PatientID;
                var patientResponse = _patientService.GetPatientByIdForPushNotificatons(apptmntpatientid, token);
                //var patientResponse = _patientService.GetPatientById(2803, token);
                PatientDemographicsModel patient;
                if (patientResponse.StatusCode == (int)HttpStatusCodes.OK)
                    patient = (PatientDemographicsModel)patientResponse.data;
                else
                    patient = null;

                string PatientfullName = patient.FirstName + " " + patient.LastName;
                int appmnttstaffid = patientAppointmentModel.AppointmentStaffs.Select(x => x.StaffId).FirstOrDefault();

                var result = _context.SymptomatePatientReport.Where(s => s.PatientID == patientAppointmentModel.PatientAppointmentId).Select(s => s.Id).FirstOrDefault();
                if (result != 0)
                {
                    IsSymptomateReportExist = true;
                    ReportId = result;
                }

                int userId = _context.Staffs.Where(x => x.Id == appmnttstaffid && x.IsActive == true && x.IsDeleted == false).Select(x => x.UserID).FirstOrDefault();
                string deviceToken = _context.User.Where(x => x.Id == userId).Select(x => x.DeviceToken).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(deviceToken))
                {
                    PushMobileNotificationModel pushMobileNotification = new PushMobileNotificationModel();
                    pushMobileNotification.DeviceToken = deviceToken;
                    pushMobileNotification.Message = "Your Appointment has been rescheduled";
                    pushMobileNotification.NotificationPriority = PushNotificationPriority.High;
                    pushMobileNotification.NotificationType = CommonEnum.NotificationActionType.UpdateAppointment.ToString();
                    PushNotificationsUserDetailsModel model = new PushNotificationsUserDetailsModel()
                    {
                        ProviderID = appmnttstaffid,
                        PatientID = (int)patientAppointmentModel.PatientID,
                        AppointmentId = patientAppointmentModel.PatientAppointmentId,
                        ImageThumbnail = "",
                        Name = PatientfullName,
                        Address = "",
                        StartDate = patientAppointmentModel.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        EndDate = patientAppointmentModel.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        StatusName = "Rescheduled",
                        IsSymptomateReportExist = IsSymptomateReportExist,
                        ReportId = ReportId

                    };
                    pushMobileNotification.Data = model;
                    PushNotificationsService.SendPushNotificationForMobile(pushMobileNotification);
                }

            }
            
            return response;
        }

        public JsonModel BookNewFreeAppointementFromPatientPortal(PatientAppointmentModel patientAppointmentModel, TokenModel token)
        {
            JsonModel response = SaveFreeAppointmentFromPatientPortal(patientAppointmentModel, token);
            if (response.StatusCode == (int)HttpStatusCodes.OK)
            {
                int.TryParse(response.data.ToString(), out int appointmentId);

                if (appointmentId <= 0)
                    return response;

                //AppointmentPaymentModel appointmentPaymentModel = new AppointmentPaymentModel()
                //{
                //    Amount = patientAppointmentModel.PayRate,
                //    AppointmentId = appointmentId,
                //    PaymentId = 0,
                //    PaymentMode = patientAppointmentModel.PaymentMode,
                //    PaymentToken = patientAppointmentModel.PaymentToken
                //};

                //var paymentResponse = _appointmentPaymentService.SaveUpdateAppointmentPayment(appointmentPaymentModel, token);
                //if (paymentResponse.StatusCode != (int)HttpStatusCodes.OK)
                //return paymentResponse;

                var staffResponse = _staffService.GetStaffProfileData(patientAppointmentModel.AppointmentStaffs[0].StaffId, token);
                StaffProfileModel staff;

                if (staffResponse.StatusCode == (int)HttpStatusCodes.OK)
                    staff = (StaffProfileModel)staffResponse.data;
                else
                    staff = null;

                var patientResponse = _patientService.GetPatientById(token.StaffID, token);
                PatientDemographicsModel patient;
                if (patientResponse.StatusCode == (int)HttpStatusCodes.OK)
                    patient = (PatientDemographicsModel)patientResponse.data;
                else
                    patient = null;

                if (staff != null)
                {
                    string error = SendAppointmentEmail(patientAppointmentModel,
                        staff.Email,
                         staff.FirstName + " " + staff.LastName,
                         (int)EmailType.BookAppointment,
                         (int)EmailSubType.BookAppointmentToProvider,
                         appointmentId,
                         "/templates/book-appointment-provider.html",
                         "New Appoointment Booked",
                         token,
                         token.Request.Request
                         );
                    //if (!string.IsNullOrEmpty(error)) response.Message = error;
                }
                if (patient != null)
                {

                    string error = SendAppointmentEmail(patientAppointmentModel,
                        patient.Email,
                         patient.FirstName + " " + patient.LastName,
                         (int)EmailType.BookAppointment,
                         (int)EmailSubType.BookAppointmentToClient,
                         appointmentId,
                         "/templates/book-appointment-client.html",
                         "Appoointment Booked Successfully",
                         token,
                         token.Request.Request
                         );
                    //if (!string.IsNullOrEmpty(error)) response.Message = error;
                }

                int staffId = patientAppointmentModel.AppointmentStaffs.Select(x => x.StaffId).FirstOrDefault();
                PatientAppointment appointmentdetails = new PatientAppointment();
                appointmentdetails = _context.PatientAppointment.Where(x => x.Id == appointmentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                string fullName = patient.FirstName + " " + patient.LastName;
                string message = _notificationRepository.GetNotificationMessage(fullName, null, appointmentId, "requested");
                PushNotificationModel saveNotificationModel = new PushNotificationModel()
                {
                    Message = message,
                    NotificationTypeId = (int)NotificationType.PushNotification,
                    TypeId = NotificationActionType.RequestAppointment,
                    SubTypeId = NotificationActionSubType.RequestAppointment,
                    StaffId = staffId,
                    NotificationType = NotificationType.PushNotification
                };
                _notificationService.SaveNotification(saveNotificationModel, token);



                if (patientAppointmentModel.IsSymptomaticProcess == false)
                {
                    //Push notifications for mobile device
                    int userId = _context.Staffs.Where(x => x.Id == staffId && x.IsActive == true && x.IsDeleted == false).Select(x => x.UserID).FirstOrDefault();
                    string deviceToken = _context.User.Where(x => x.Id == userId).Select(x => x.DeviceToken).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(deviceToken))
                    {
                        PushMobileNotificationModel pushMobileNotification = new PushMobileNotificationModel();
                        pushMobileNotification.DeviceToken = deviceToken;
                        pushMobileNotification.Message = message;
                        pushMobileNotification.NotificationPriority = PushNotificationPriority.High;
                        pushMobileNotification.NotificationType = CommonEnum.NotificationActionType.RequestAppointment.ToString();
                        PushNotificationsUserDetailsModel model = new PushNotificationsUserDetailsModel()
                        {
                            ProviderID = staffId,
                            PatientID = (int)appointmentdetails.PatientID,
                            AppointmentId = appointmentdetails.Id,
                            ImageThumbnail = string.IsNullOrEmpty(patient.PhotoThumbnailPath) ? patient.PhotoThumbnailPath : CommonMethods.CreateImageUrl(token.Request, ImagesPath.PatientThumbPhotos, patient.PhotoThumbnailPath),
                            Name = fullName,
                            Address = patientAppointmentModel.Address,
                            StartDate = appointmentdetails.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                            EndDate = appointmentdetails.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                            StatusName = appointmentdetails.AppointmentStatus.GlobalCodeName,
                            IsTelehealthAppointment = appointmentdetails.IsTelehealthAppointment,
                            AppointmentTypeName = appointmentdetails.BookingType
                        };
                        pushMobileNotification.Data = model;
                        PushNotificationsService.SendPushNotificationForMobile(pushMobileNotification);
                    }
                }

            }
            return response;
        }

        public JsonModel SaveFreeAppointmentFromPatientPortal(PatientAppointmentModel patientAppointmentModel, TokenModel token)
        {

            var organizationJsonModel = _organizationService.GetOrganizationDetailsById(token);
            LocationModel locationModal = _locationService.GetLocationOffsets(patientAppointmentModel.ServiceLocationID, token);
            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    PatientAppointment patientAppointment = null;
                    AppointmentStaff appointmentStaff = null;
                    // GetLocationOffsets(patientAppointmentModel.ServiceLocationID);
                    if (patientAppointmentModel.PatientAppointmentId == 0)
                    {
                        patientAppointment = new PatientAppointment();
                        AutoMapper.Mapper.Map(patientAppointmentModel, patientAppointment);
                        if (patientAppointment.AppointmentTypeID == 0)
                            patientAppointment.AppointmentTypeID = null;
                        patientAppointment.StartDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointment.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                        patientAppointment.EndDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointment.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                        patientAppointment.OrganizationID = token.OrganizationID;
                        patientAppointment.IsActive = true;
                        patientAppointment.CreatedBy = token.UserID;
                        patientAppointment.CreatedDate = DateTime.UtcNow;
                        patientAppointment.Offset = (int)CommonMethods.GetCurrentOffset(patientAppointment.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                        patientAppointment.StatusId = _globalCodeService.GetGlobalCodeValueId(GlobalCodeName.AppointmentStatus, AppointmentStatus.PENDING, token);
                        patientAppointment.IsClientRequired = true;
                        patientAppointment.BookingMode = patientAppointmentModel.Mode;
                        patientAppointment.BookingType = patientAppointmentModel.Type;
                        _appointmentRepository.Create(patientAppointment);
                        _appointmentRepository.SaveChanges();

                        appointmentStaff = patientAppointmentModel.AppointmentStaffs.Select(x => new AppointmentStaff() { StaffID = x.StaffId, PatientAppointmentID = patientAppointment.Id, CreatedBy = token.UserID, CreatedDate = DateTime.UtcNow, IsActive = true }).FirstOrDefault();
                        _appointmentStaffRepository.Create(appointmentStaff);
                        _appointmentStaffRepository.SaveChanges();

                        response = new JsonModel(patientAppointment.Id, StatusMessage.AddPatientAppointment, (int)HttpStatusCodes.OK, string.Empty);
                    }
                    else
                    {
                        patientAppointment = _appointmentRepository.Get(x => x.Id == patientAppointmentModel.PatientAppointmentId && x.IsActive == true && x.IsDeleted == false);
                        if (!ReferenceEquals(patientAppointment, null))
                        {
                            patientAppointment.StartDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointmentModel.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                            patientAppointment.EndDateTime = CommonMethods.ConvertToUtcTimeWithOffset(patientAppointmentModel.EndDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                            ///patientAppointment.AppointmentTypeID = patientAppointmentModel.AppointmentTypeID;
                            patientAppointment.AppointmentTypeID = null;
                            patientAppointment.Notes = patientAppointmentModel.Notes;
                            patientAppointment.PatientAddressID = patientAppointmentModel.PatientAddressID;
                            patientAppointment.ServiceLocationID = patientAppointmentModel.ServiceLocationID;
                            patientAppointment.OfficeAddressID = patientAppointmentModel.OfficeAddressID;
                            patientAppointment.CustomAddress = patientAppointmentModel.CustomAddress;
                            patientAppointment.CustomAddressID = patientAppointmentModel.CustomAddressID;
                            patientAppointment.Longitude = patientAppointmentModel.Longitude;
                            patientAppointment.Latitude = patientAppointmentModel.Latitude;
                            patientAppointment.ApartmentNumber = patientAppointmentModel.ApartmentNumber;
                            //patientAppointment.IsDirectService = patientAppointmentModel.IsDirectService;
                            patientAppointment.IsTelehealthAppointment = true;
                            patientAppointment.IsDirectService = true;
                            patientAppointment.IsExcludedFromMileage = true;
                            patientAppointment.IsClientRequired = true;
                            patientAppointment.UpdatedBy = token.UserID;
                            patientAppointment.UpdatedDate = DateTime.UtcNow;
                            patientAppointment.Offset = (int)CommonMethods.GetCurrentOffset(patientAppointmentModel.StartDateTime, locationModal.DaylightOffset, locationModal.StandardOffset, locationModal.TimeZoneName);
                            _appointmentRepository.Update(patientAppointment);
                            _appointmentRepository.SaveChanges();

                            appointmentStaff = _appointmentStaffRepository.GetAll(x => x.PatientAppointmentID == patientAppointmentModel.PatientAppointmentId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (appointmentStaff != null)
                            {
                                appointmentStaff.StaffID = patientAppointmentModel.AppointmentStaffs.FirstOrDefault().StaffId;
                                appointmentStaff.UpdatedBy = token.UserID;
                                appointmentStaff.UpdatedDate = DateTime.UtcNow;
                                _appointmentStaffRepository.Update(appointmentStaff);
                                _appointmentStaffRepository.SaveChanges();
                            }
                            response = new JsonModel() { Message = StatusMessage.UpdatePatientAppointment };
                        }

                        response.data = null;
                        response.StatusCode = (int)HttpStatusCodes.OK;


                    }
                    //if (response.StatusCode == (int)HttpStatusCodes.OK && !string.IsNullOrEmpty(patientAppointmentModel.PaymentToken))
                    if (response.StatusCode == (int)HttpStatusCodes.OK)
                    {
                        #region Payment Save
                        if (organizationJsonModel.StatusCode != (int)HttpStatusCode.OK)
                            throw new Exception(StatusMessage.NotFound);

                        var organization = (OrganizationDetailModel)organizationJsonModel.data;
                        if (organization == null)
                            throw new Exception(StatusMessage.NotFound);


                        //StripeConfiguration.ApiKey = organization.StripeSecretKey;
                        //var tokenService = new TokenService();
                        //Stripe.Token stripeToken = tokenService.Get(patientAppointmentModel.PaymentToken);

                        //var options = new ChargeCreateOptions
                        //{
                        //    Amount = (long?)patientAppointmentModel.PayRate * 100,
                        //    Currency = "inr",
                        //    Source = stripeToken.Id,
                        //    Description = "Appointment Fee",
                        //    ReceiptEmail = stripeToken.Card.Name,
                        //    Shipping = new ChargeShippingOptions
                        //    {
                        //        Address = new AddressOptions
                        //        {
                        //            Country = stripeToken.Card.Country,
                        //            City = "Chandigarh",
                        //            Line1 = "House No 42",
                        //            Line2 = "Mohali",
                        //            PostalCode = "0123065",
                        //            State = "Punjab"
                        //        },
                        //        Name = stripeToken.Card.Name
                        //    }

                        //};
                        //var service = new ChargeService();
                        //var charge = service.Create(options);

                        AppointmentPaymentModel appointmentPaymentModel = new AppointmentPaymentModel()
                        {
                            Amount = 0,
                            AppointmentId = patientAppointment.Id,
                            PaymentId = 0,
                            PaymentMode = patientAppointmentModel.PaymentMode,
                            PaymentToken = ""
                        };

                        //if (appointmentPaymentModel.PaymentId == 0)
                        //{
                        //    var tokenPayment = _appointmentPaymentRepository.GetAppointmentPaymentsByPaymentToken(appointmentPaymentModel.PaymentToken, token);
                        //    if (tokenPayment != null)
                        //        throw new Exception(StatusMessage.AppointmentPaymentTokenExisted);
                        //}
                        var appointmentPayment = _mapper.Map<AppointmentPayments>(appointmentPaymentModel);
                        appointmentPayment.OrganizationId = token.OrganizationID;
                        appointmentPayment.CommissionPercentage = organization.BookingCommision;
                        appointmentPayment.CreatedBy = token.UserID;
                        appointmentPayment.CreatedDate = DateTime.UtcNow;
                        _appointmentPaymentRepository.Create(appointmentPayment);
                        _appointmentPaymentRepository.SaveChanges();


                        #endregion Payment Save
                    }
                    transaction.Commit();


                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    response = new JsonModel(null, StatusMessage.ServerError, (int)HttpStatusCodes.InternalServerError, ex.Message);
                }
            }
            return response;
        }

    }
}